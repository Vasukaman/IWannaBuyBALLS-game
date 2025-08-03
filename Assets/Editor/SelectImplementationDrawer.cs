// Filename: SelectImplementationDrawer.cs (MUST be in an "Editor" folder)
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SelectImplementationAttribute))]
public class SelectImplementationDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var attribute = this.attribute as SelectImplementationAttribute;
        var fieldType = attribute.FieldType;

        // --- 1. Draw the Main Label and Dropdown ---
        Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        var implementationTypes = TypeCache.GetTypesDerivedFrom(fieldType)
            .Where(t => !t.IsAbstract && !t.IsInterface).ToList();

        var typeNames = implementationTypes.Select(t => t.Name).ToList();
        typeNames.Insert(0, "(None)"); // Add an option for null

        int currentIndex = 0;
        if (property.managedReferenceValue != null)
        {
            currentIndex = implementationTypes.FindIndex(t => t == property.managedReferenceValue.GetType()) + 1;
        }

        int newIndex = EditorGUI.Popup(dropdownRect, label.text, currentIndex, typeNames.ToArray());

        if (newIndex != currentIndex)
        {
            if (newIndex == 0) // "(None)" was selected
            {
                property.managedReferenceValue = null;
            }
            else
            {
                var newType = implementationTypes[newIndex - 1];
                property.managedReferenceValue = Activator.CreateInstance(newType);
            }
        }

        // --- 2. Draw the Properties of the Selected Object (No Fold-out) ---
        if (property.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;

            // This iterates through the children of the serialized object (e.g., "AmountToAdd")
            foreach (SerializedProperty child in property)
            {
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                Rect childRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(childRect, child, true);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    // This method is crucial for telling the Inspector how much vertical space our custom UI needs.
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }
}