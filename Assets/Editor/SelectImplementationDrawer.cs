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

        // Find all concrete types that implement the interface
        var implementationTypes = TypeCache.GetTypesDerivedFrom(fieldType)
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .ToList();

        var typeNames = implementationTypes.Select(t => t.Name).ToList();
        int currentIndex = -1;
        if (property.managedReferenceValue != null)
        {
            currentIndex = implementationTypes.FindIndex(t => t == property.managedReferenceValue.GetType());
        }

        // Draw the dropdown menu
        position.height = EditorGUIUtility.singleLineHeight;
        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, typeNames.ToArray());

        // If the user selects a new type, create a new instance
        if (newIndex != currentIndex)
        {
            var newType = implementationTypes[newIndex];
            property.managedReferenceValue = Activator.CreateInstance(newType);
        }

        // Draw the properties of the currently selected object
        EditorGUI.PropertyField(position, property, label, true);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }
}