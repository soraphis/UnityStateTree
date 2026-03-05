using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

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
            AddChildState(_selectedPath);
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
            _addChildButton?.SetEnabled(_selectedPath != null);
            _deleteStateButton?.SetEnabled(_selectedPath != null && !IsRootPath(_selectedPath));
        }

        // ── State list ────────────────────────────────────────────────────────

        private void RefreshStateList()
        {
            _stateListView.Clear();
            serializedObject.Update();
            var rootProp = serializedObject.FindProperty(RootPath);
            if (rootProp == null) return;
            RenderStateRecursive(rootProp, RootPath, 0);
        }

        private void RenderStateRecursive(SerializedProperty stateProp, string path, int depth)
        {
            var row = new VisualElement();
            row.AddToClassList("state-row");
            row.style.paddingLeft = depth * 12;

            var box = new VisualElement();
            box.AddToClassList("state-box");
            box.pickingMode = PickingMode.Ignore;

            if (_selectedPath == path)
                box.AddToClassList("selected");

            box.Add(new Label(GetStateName(stateProp)));
            row.Add(box);

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
                evt.menu.AppendAction("Add Child State", _ => AddChildState(capturedPath));
                evt.menu.AppendAction("Delete State",
                    _ => DeleteState(capturedPath),
                    IsRootPath(capturedPath)
                        ? DropdownMenuAction.Status.Disabled
                        : DropdownMenuAction.Status.Normal);
            }));

            _stateListView.Add(row);

            // Recurse into children
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

        // ── Mutations (SerializedProperty only) ───────────────────────────────

        private void AddChildState(string parentPath)
        {
            serializedObject.Update();

            var childrenProp = serializedObject.FindProperty($"{parentPath}.children");
            if (childrenProp == null) return;

            childrenProp.InsertArrayElementAtIndex(childrenProp.arraySize);
            var newChild = childrenProp.GetArrayElementAtIndex(childrenProp.arraySize - 1);

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
            AddBoundField(prop, "selectionBehavior");
            AddBoundField(prop, "tasks", "Tasks");
            AddBoundField(prop, "transitions", "Transitions");
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
    }
}
