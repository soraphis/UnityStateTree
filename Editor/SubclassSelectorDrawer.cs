using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace StateTree.Editor.Editor
{
    /// <summary>
    /// Generic property drawer for [SerializeReference] fields that shows a dropdown
    /// to select a concrete subclass. Applied to Task and Transition types.
    /// </summary>
    internal static class SubclassSelectorUtility
    {
        private static readonly Dictionary<Type, Type[]> SubclassCache = new();

        public static Type[] GetSubclasses(Type baseType)
        {
            if (SubclassCache.TryGetValue(baseType, out var cached))
                return cached;

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .Where(t => !t.IsAbstract && !t.IsInterface && baseType.IsAssignableFrom(t) && t != baseType)
                .OrderBy(t => t.FullName)
                .ToArray();

            SubclassCache[baseType] = types;
            return types;
        }

        /// <summary>
        /// Creates a VisualElement for a [SerializeReference] property that includes
        /// a type-selector dropdown followed by the normal PropertyField for the selected instance.
        /// </summary>
        public static VisualElement CreateSubclassSelectorElement(SerializedProperty property, SerializedObject serializedObject, Type baseType, string label = null)
        {
            var container = new VisualElement();
            container.style.marginBottom = 6;

            var subclasses = GetSubclasses(baseType);
            var choices = new List<string> { "<None>" };
            choices.AddRange(subclasses.Select(FormatTypeName));

            var currentType = property.managedReferenceValue?.GetType();
            var currentIndex = currentType != null ? Array.IndexOf(subclasses, currentType) + 1 : 0;

            var typeLabel = label ?? ObjectNames.NicifyVariableName(baseType.Name);
            var popup = new PopupField<string>(typeLabel, choices, currentIndex);
            popup.AddToClassList("unity-base-field");

            // PropertyField for the current instance (re-created when type changes)
            var fieldContainer = new VisualElement();
            RefreshField(fieldContainer, property, serializedObject);

            popup.RegisterValueChangedCallback(evt =>
            {
                var selectedIndex = choices.IndexOf(evt.newValue);
                var targetType = selectedIndex > 0 ? subclasses[selectedIndex - 1] : null;
                var current = property.managedReferenceValue;

                if (targetType == null)
                {
                    if (current != null)
                    {
                        property.managedReferenceValue = null;
                        serializedObject.ApplyModifiedProperties();
                        RefreshField(fieldContainer, property, serializedObject);
                    }
                    return;
                }

                if (current?.GetType() == targetType) return;

                property.managedReferenceValue = Activator.CreateInstance(targetType);
                serializedObject.ApplyModifiedProperties();
                RefreshField(fieldContainer, property, serializedObject);
            });

            container.Add(popup);
            container.Add(fieldContainer);
            return container;
        }

        private static void RefreshField(VisualElement fieldContainer, SerializedProperty property, SerializedObject serializedObject)
        {
            fieldContainer.Clear();
            if (property.managedReferenceValue == null) return;

            // Re-find the property so it reflects the new type
            var freshProp = serializedObject.FindProperty(property.propertyPath);
            if (freshProp == null) return;

            var pf = new PropertyField(freshProp);
            pf.Bind(serializedObject);
            fieldContainer.Add(pf);
        }

        private static string FormatTypeName(Type t)
        {
            // Strip namespace, nicify
            return ObjectNames.NicifyVariableName(t.Name);
        }
    }

    [CustomPropertyDrawer(typeof(UnityStateTree.Task), true)]
    public class TaskSubclassSelectorDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return SubclassSelectorUtility.CreateSubclassSelectorElement(
                property,
                property.serializedObject,
                typeof(UnityStateTree.Task));
        }

        // IMGUI fallback (height 0 — we use UIElements only)
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position, label.text, "(Use Inspector UIElements)");
        }
    }

    [CustomPropertyDrawer(typeof(UnityStateTree.Transition), true)]
    public class TransitionSubclassSelectorDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return SubclassSelectorUtility.CreateSubclassSelectorElement(
                property,
                property.serializedObject,
                typeof(UnityStateTree.Transition));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position, label.text, "(Use Inspector UIElements)");
        }
    }
    
    [CustomPropertyDrawer(typeof(UnityStateTree.Condition), true)]
    public class ConditionSubclassSelectorDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return SubclassSelectorUtility.CreateSubclassSelectorElement(
                property,
                property.serializedObject,
                typeof(UnityStateTree.Condition));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position, label.text, "(Use Inspector UIElements)");
        }
    }
}

