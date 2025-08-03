// Filename: SpringTransformProfile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpringTransformProfile", menuName = "Profiles/Core/Spring Transform Profile")]
public class SpringTransformProfile : ScriptableObject
{
    [Header("Spring Settings")]
    [Tooltip("How 'springy' the animation is. Higher values are faster and more bouncy.")]
    public float Stiffness = 20f;

    [Tooltip("How much the wobble is dampened. 0 is infinite wobble, 1 is no wobble.")]
    [Range(0, 1)]
    public float Damping = 0.7f;
}