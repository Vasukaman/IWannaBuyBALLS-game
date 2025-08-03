// Filename: SelectImplementationAttribute.cs
using Gameplay.Gadgets.Effects;
using System;
using UnityEngine;

// This is just a "marker" for our drawer to find.
public class SelectImplementationAttribute : PropertyAttribute
{
    public Type FieldType;
    public SelectImplementationAttribute(Type fieldType) => FieldType = fieldType;
}
