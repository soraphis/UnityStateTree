using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityStateTree;

namespace StateTree.Editor.Editor
{
    [CustomEditor(typeof(UnityStateTree.StateTreeAsset))]
    public class StateTreeAssetEditor : UnityEditor.Editor
    {
        private VisualElement _root;
        private ScrollView _stateListView;
        private VisualElement _detailDrawer;

        // Path into serializedObject that identifies the selected state.
        // e.g. "stateTree.rootState" or "stateTree.rootState.children.Array.data[0]"
        private string _selectedPath;

        private Button _addChildButton;
        private Button _deleteStateButton;

        private StyleSheet _styleSheet;

        private void OnEnable()
        {
            _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Packages/com.soraphis.unitystatetree/Editor/state-tree-editor.uss");
            if (_styleSheet == null)
                Debug.LogError("Failed to load USS");
        }

        // ── Root path helpers ─────────────────────────────────────────────────

        private const string RootPath = "stateTree.rootState";

        private bool IsRootPath(string path) => path == RootPath;

        /// Returns the SerializedProperty for the currently selected state (or null).
        private SerializedProperty SelectedProperty =>
            _selectedPath != null ? serializedObject.FindProperty(_selectedPath) : null;

        /// Returns the display name of a state property.
        private static string GetStateName(SerializedProperty stateProp)
        {
            var n = stateProp.FindPropertyRelative("name")?.stringValue;
            return string.IsNullOrEmpty(n) ? "<unnamed>" : n;
        }

        // ── Inspector GUI ─────────────────────────────────────────────────────

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            if (_styleSheet) _root.styleSheets.Add(_styleSheet);
            _root.AddToClassList("state-tree-root");

            // Toolbar
            var toolbar = new Toolbar();

            _addChildButton = new ToolbarButton(OnAddChildClicked) { text = "Add Child State" };
            _addChildButton.tooltip = "Adds a new child state to the currently selected state";
            _addChildButton.SetEnabled(false);
            toolbar.Add(_addChildButton);

            _deleteStateButton = new ToolbarButton(OnDeleteStateClicked) { text = "Delete State" };
            _deleteStateButton.tooltip = "Deletes the currently selected state and all its children";
            _deleteStateButton.SetEnabled(false);
            toolbar.Add(_deleteStateButton);

            _root.Add(toolbar);

            _stateListView = new ScrollView();
            _stateListView.AddToClassList("state-list");
            _root.Add(_stateListView);

            var drawerBox = new Box();
            drawerBox.AddToClassList("detail-drawer");
            drawerBox.AddToClassList("inspector-box");
            _detailDrawer = drawerBox;
            _root.Add(_detailDrawer);

            Refresh();
            return _root;
        }

        // ── Toolbar callbacks ─────────────────────────────────────────────────

        private void OnAddChildClicked()
        {
            if (_selectedPath == null) return;
            ShowAddChildPopup(_selectedPath);
        }

        private void ShowAddChildPopup(string parentPath)
        {
            var subclasses = SubclassSelectorUtility.GetSubclasses(typeof(StateEntry));
            var menu = new GenericMenu();
            foreach (var type in subclasses)
            {
                var capturedType = type;
                menu.AddItem(
                    new GUIContent(ObjectNames.NicifyVariableName(type.Name)),
                    false,
                    () => AddChildState(parentPath, capturedType));
            }
            menu.ShowAsContext();
        }

        private void OnDeleteStateClicked()
        {
            if (_selectedPath == null || IsRootPath(_selectedPath)) return;
            DeleteState(_selectedPath);
        }

        // ── Full refresh ──────────────────────────────────────────────────────

        private void Refresh()
        {
            serializedObject.Update();
            RefreshStateList();
            RenderDetailDrawer();
            UpdateToolbarButtons();
        }

        private void UpdateToolbarButtons()
        {
            var selectedProp = _selectedPath != null ? serializedObject.FindProperty(_selectedPath) : null;
            var isSelectorState = selectedProp?.managedReferenceValue is SelectorState;
            _addChildButton?.SetEnabled(isSelectorState);
            _deleteStateButton?.SetEnabled(_selectedPath != null && !IsRootPath(_selectedPath));
        }

        // ── State list ────────────────────────────────────────────────────────

        private void RefreshStateList()
        {
            _stateListView.Clear();
            serializedObject.Update();
            var rootProp = serializedObject.FindProperty(RootPath);
            if (rootProp == null) return;
            if (rootProp.managedReferenceValue == null)
            {
                rootProp.managedReferenceValue = new UnityStateTree.SelectInOrder { name = "Root", depth = 0 };
                serializedObject.ApplyModifiedProperties();
            }
            RenderStateRecursive(rootProp, RootPath, 0);
        }

        private void RenderStateRecursive(SerializedProperty stateProp, string path, int depth)
        {
            var row = new VisualElement();
            row.AddToClassList("state-row");
            row.style.paddingLeft = PixelDepth(depth);

            var selectionBox = new VisualElement();
            selectionBox.AddToClassList("selection-box");
            selectionBox.pickingMode = PickingMode.Ignore;
            selectionBox.style.flexDirection = FlexDirection.Row;
            selectionBox.style.alignItems = Align.Center;

            if (_selectedPath == path)
                selectionBox.AddToClassList("selected");

            var box = new VisualElement();
            box.AddToClassList("state-box");
            box.pickingMode = PickingMode.Ignore;

            if (stateProp.managedReferenceValue is SelectorState selectorState)
            {
                box.AddToClassList("selector-state");
                box.Add(new Label(GetStateName(stateProp) + ": " + selectorState.GetType().Name));
            }
            else
            {
                box.Add(new Label(GetStateName(stateProp)));
            }

            selectionBox.Add(box);

            // ── Conditions chips (inline, next to state box) ──────────────────
            var conditionsProp = stateProp.FindPropertyRelative("entryConditions");
            if (conditionsProp != null && conditionsProp.arraySize > 0)
            {
                selectionBox.Add(new Label(" IF "){style =
                {
                    marginLeft = 5, marginRight = 5,
                }});
                
                var conditionsBox = new VisualElement();
                conditionsBox.AddToClassList("conditions-box");
                conditionsBox.pickingMode = PickingMode.Ignore;
                conditionsBox.style.flexDirection = FlexDirection.Row;
                conditionsBox.style.flexWrap = Wrap.Wrap;
                for (int i = 0; i < conditionsProp.arraySize; i++)
                {
                    var condProp = conditionsProp.GetArrayElementAtIndex(i);
                    var condName = condProp.managedReferenceValue is Condition c
                        ? c.GetDescription()
                        : "<null>";
                    var chip = new Label(condName);
                    chip.AddToClassList("condition-chip");
                    chip.pickingMode = PickingMode.Ignore;
                    conditionsBox.Add(chip);
                }
                selectionBox.Add(conditionsBox);
            }

            row.Add(selectionBox);
            row.pickingMode = PickingMode.Position;
            row.focusable = true;

            // Capture path for callbacks
            var capturedPath = path;
            row.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == (int)MouseButton.LeftMouse)
                {
                    _selectedPath = capturedPath;
                    Refresh();
                }
            });

            row.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                _selectedPath = capturedPath;
                Refresh();
                var canAddChild = stateProp.managedReferenceValue is SelectorState;
                evt.menu.AppendAction("Add Child State",
                    _ => ShowAddChildPopup(capturedPath),
                    canAddChild
                        ? DropdownMenuAction.Status.Normal
                        : DropdownMenuAction.Status.Disabled);
                evt.menu.AppendAction("Delete State",
                    _ => DeleteState(capturedPath),
                    IsRootPath(capturedPath)
                        ? DropdownMenuAction.Status.Disabled
                        : DropdownMenuAction.Status.Normal);
            }));

            _stateListView.Add(row);

            // ── Tasks sub-rows ────────────────────────────────────────────────
            var tasksProp = stateProp.FindPropertyRelative("tasks");
            if (tasksProp != null && tasksProp.arraySize > 0)
            {
                var tasksRow = new VisualElement();
                tasksRow.AddToClassList("tasks-row");
                tasksRow.style.paddingLeft = PixelDepth(depth) + 16;
                tasksRow.style.flexDirection = FlexDirection.Row;
                tasksRow.style.flexWrap = Wrap.Wrap;
                tasksRow.pickingMode = PickingMode.Ignore;
                for (int i = 0; i < tasksProp.arraySize; i++)
                {
                    var taskProp = tasksProp.GetArrayElementAtIndex(i);
                    var taskName = taskProp.managedReferenceValue != null
                        ? ObjectNames.NicifyVariableName(taskProp.managedReferenceValue.GetType().Name)
                        : "<null>";
                    var chip = new Label("▸ " + taskName);
                    chip.AddToClassList("task-chip");
                    chip.pickingMode = PickingMode.Ignore;
                    tasksRow.Add(chip);
                }
                _stateListView.Add(tasksRow);
            }

            // ── Transitions sub-rows ──────────────────────────────────────────
            var transitionsProp = stateProp.FindPropertyRelative("transitions");
            if (transitionsProp != null && transitionsProp.arraySize > 0)
            {
                var transitionsRow = new VisualElement();
                transitionsRow.AddToClassList("transitions-row");
                transitionsRow.style.paddingLeft = PixelDepth(depth) + 16;
                transitionsRow.style.flexDirection = FlexDirection.Row;
                transitionsRow.style.flexWrap = Wrap.Wrap;
                transitionsRow.pickingMode = PickingMode.Ignore;
                for (int i = 0; i < transitionsProp.arraySize; i++)
                {
                    var transProp = transitionsProp.GetArrayElementAtIndex(i);
                    var (icon, extraClass) = GetTransitionIconAndClass(transProp);
                    var transName = transProp.managedReferenceValue != null
                        ? ObjectNames.NicifyVariableName(transProp.managedReferenceValue.GetType().Name)
                        : "<null>";
                    var chip = new Label(icon + " " + transName);
                    chip.AddToClassList("transition-chip");
                    if (extraClass != null) chip.AddToClassList(extraClass);
                    chip.pickingMode = PickingMode.Ignore;
                    transitionsRow.Add(chip);
                }
                _stateListView.Add(transitionsRow);
            }

            // ── Recurse into children ─────────────────────────────────────────
            var childrenProp = stateProp.FindPropertyRelative("children");
            if (childrenProp == null) return;
            for (int i = 0; i < childrenProp.arraySize; i++)
            {
                var childPath = $"{path}.children.Array.data[{i}]";
                var childProp = serializedObject.FindProperty(childPath);
                if (childProp != null)
                    RenderStateRecursive(childProp, childPath, depth + 1);
            }
        }

        private static int PixelDepth(int depth)
        {
            return depth * 24;
        }

        // ── Mutations (SerializedProperty only) ───────────────────────────────

        /// Returns the icon glyph and an optional extra CSS class for a transition chip.
        private static (string icon, string extraClass) GetTransitionIconAndClass(SerializedProperty transProp)
        {
            if (transProp.managedReferenceValue is TransitionSimple simple)
            {
                return simple.targetType switch
                {
                    TransitionSimple.TransitionTargetType.ToRoot    => ("⤴", "transition-chip--to-root"),
                    TransitionSimple.TransitionTargetType.ToParent  => ("↑", "transition-chip--to-parent"),
                    TransitionSimple.TransitionTargetType.ToNextSibling => ("→", null),
                    _ => ("⇒", null),
                };
            }
            return ("⇒", null);
        }

        private void AddChildState(string parentPath, Type stateType)
        {
            serializedObject.Update();

            var childrenProp = serializedObject.FindProperty($"{parentPath}.children");
            if (childrenProp == null) return;

            childrenProp.InsertArrayElementAtIndex(childrenProp.arraySize);
            var newChild = childrenProp.GetArrayElementAtIndex(childrenProp.arraySize - 1);

            // Assign managed reference first so relative properties resolve correctly
            newChild.managedReferenceValue = System.Activator.CreateInstance(stateType);

            // Initialise the new element's fields
            newChild.FindPropertyRelative("name").stringValue = "New State";

            // Clear lists so they don't inherit garbage from a previous array element
            ClearList(newChild.FindPropertyRelative("children"));
            ClearList(newChild.FindPropertyRelative("entryConditions"));
            ClearList(newChild.FindPropertyRelative("tasks"));
            ClearList(newChild.FindPropertyRelative("transitions"));

            var newChildPath = $"{parentPath}.children.Array.data[{childrenProp.arraySize - 1}]";

            serializedObject.ApplyModifiedProperties();

            _selectedPath = newChildPath;
            Refresh();
        }

        private void DeleteState(string path)
        {
            if (IsRootPath(path)) return;

            serializedObject.Update();

            // Derive parent path and index from the path string
            // Path ends with ".children.Array.data[N]"
            var arrayDataMarker = ".children.Array.data[";
            int markerIdx = path.LastIndexOf(arrayDataMarker, System.StringComparison.Ordinal);
            if (markerIdx < 0) return;

            var parentPath = path.Substring(0, markerIdx);
            var indexStr = path.Substring(markerIdx + arrayDataMarker.Length).TrimEnd(']');
            if (!int.TryParse(indexStr, out int index)) return;

            var childrenProp = serializedObject.FindProperty($"{parentPath}.children");
            if (childrenProp == null || index < 0 || index >= childrenProp.arraySize) return;

            childrenProp.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();

            _selectedPath = null;
            Refresh();
        }

        private static void ClearList(SerializedProperty listProp)
        {
            if (listProp != null)
                listProp.ClearArray();
        }

        // ── Detail drawer ─────────────────────────────────────────────────────

        private void RenderDetailDrawer()
        {
            _detailDrawer.Clear();

            if (_selectedPath == null)
            {
                var lbl = new Label("No state selected");
                lbl.AddToClassList("help-box");
                _detailDrawer.Add(lbl);
                return;
            }

            serializedObject.Update();
            var prop = serializedObject.FindProperty(_selectedPath);
            if (prop == null)
            {
                _selectedPath = null;
                _detailDrawer.Add(new Label("State no longer exists."));
                return;
            }

            AddBoundField(prop, "name");
            _detailDrawer.Add(CreateManagedRefListField(prop.FindPropertyRelative("entryConditions"), "Entry Conditions", typeof(Condition)));
            AddBoundField(prop, "selectionBehavior");
            _detailDrawer.Add(CreateManagedRefListField(prop.FindPropertyRelative("tasks"), "Tasks", typeof(Task)));
            _detailDrawer.Add(CreateManagedRefListField(prop.FindPropertyRelative("transitions"), "Transitions", typeof(Transition)));
        }

        private void AddBoundField(SerializedProperty parent, string relativeName, string label = null)
        {
            var childProp = parent.FindPropertyRelative(relativeName);
            if (childProp == null) return;
            var field = label != null
                ? new PropertyField(childProp, label)
                : new PropertyField(childProp);
            field.Bind(serializedObject);
            _detailDrawer.Add(field);
        }

        /// Renders a [SerializeReference] list with an ArrayDrawer-style header (foldout,
        /// item count, type-picker "+" button) and per-item rows with a "−" delete button.
        private VisualElement CreateManagedRefListField(SerializedProperty listProp, string listLabel, Type baseType)
        {
            if (listProp == null) return new VisualElement();

            var propertyPath = listProp.propertyPath;
            var container = new VisualElement();
            container.AddToClassList("list-field-container");

            // ── Content area ──────────────────────────────────────────────
            var contentArea = new VisualElement();
            contentArea.AddToClassList("list-field-content");
            contentArea.style.display = listProp.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;

            // ── Header ────────────────────────────────────────────────────
            var foldoutLbl = new Label(listProp.isExpanded ? "▾" : "▸");
            foldoutLbl.AddToClassList("list-foldout");

            var titleLbl = new Label(listLabel);
            titleLbl.AddToClassList("list-field-title");

            var countBadge = new Label($"({listProp.arraySize})");
            countBadge.AddToClassList("list-count-badge");

            void ToggleFold()
            {
                var p = serializedObject.FindProperty(propertyPath);
                if (p == null) return;
                p.isExpanded = !p.isExpanded;
                serializedObject.ApplyModifiedProperties();
                foldoutLbl.text = p.isExpanded ? "▾" : "▸";
                contentArea.style.display = p.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            }

            foldoutLbl.RegisterCallback<ClickEvent>(_ => ToggleFold());
            titleLbl.RegisterCallback<ClickEvent>(_ => ToggleFold());

            var addBtn = new Button(() =>
            {
                var subclasses = SubclassSelectorUtility.GetSubclasses(baseType);
                var menu = new GenericMenu();
                foreach (var type in subclasses)
                {
                    var captured = type;
                    menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false, () =>
                    {
                        serializedObject.Update();
                        var p = serializedObject.FindProperty(propertyPath);
                        if (p == null) return;
                        p.InsertArrayElementAtIndex(p.arraySize);
                        p.GetArrayElementAtIndex(p.arraySize - 1).managedReferenceValue = Activator.CreateInstance(captured);
                        // Auto-expand when an item is added
                        p.isExpanded = true;
                        serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            }) { text = "+" };
            addBtn.AddToClassList("list-add-btn");

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;

            var header = new VisualElement();
            header.AddToClassList("list-field-header");
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.Add(foldoutLbl);
            header.Add(titleLbl);
            header.Add(countBadge);
            header.Add(spacer);
            header.Add(addBtn);

            // ── Content builder ───────────────────────────────────────────
            void RebuildContent()
            {
                contentArea.Clear();
                serializedObject.Update();
                var freshProp = serializedObject.FindProperty(propertyPath);
                if (freshProp == null) return;

                countBadge.text = $"({freshProp.arraySize})";
                foldoutLbl.text = freshProp.isExpanded ? "▾" : "▸";
                contentArea.style.display = freshProp.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;

                for (int i = 0; i < freshProp.arraySize; i++)
                {
                    var elemProp = freshProp.GetArrayElementAtIndex(i);
                    var row = new VisualElement();
                    row.AddToClassList("list-item-row");
                    row.style.flexDirection = FlexDirection.Row;
                    row.style.alignItems = Align.FlexStart;

                    // var pf = new PropertyField(elemProp, ""); // this does not find custom property drawers, instead:
                    var pf = new PropertyField(elemProp, "");
                    pf.AddToClassList("list-item-field");
                    pf.style.flexGrow = 1;
                    pf.Bind(serializedObject);

                    var capturedIndex = i;
                    var delBtn = new Button(() =>
                    {
                        serializedObject.Update();
                        var p = serializedObject.FindProperty(propertyPath);
                        if (p != null && capturedIndex < p.arraySize)
                        {
                            p.DeleteArrayElementAtIndex(capturedIndex);
                            serializedObject.ApplyModifiedProperties();
                        }
                    }) { text = "−" };
                    delBtn.AddToClassList("list-delete-btn");

                    row.Add(pf);
                    row.Add(delBtn);
                    contentArea.Add(row);
                }
            }

            RebuildContent();
            container.TrackPropertyValue(listProp, _ => RebuildContent());

            container.Add(header);
            container.Add(contentArea);
            return container;
        }
    }
    //
    // [CustomPropertyDrawer(typeof(Condition))]
    // public class StateTreeConditionDrawer : PropertyDrawer
    // {
    //     public override VisualElement CreatePropertyGUI(SerializedProperty property)
    //     {
    //         var field = new PropertyField(property);
    //         field.Bind(property.serializedObject);
    //         return field;
    //     }
    // }
}
