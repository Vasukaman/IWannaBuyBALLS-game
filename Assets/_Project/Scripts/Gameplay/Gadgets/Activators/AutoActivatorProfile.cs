// Filename: AutoActivatorProfile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewAutoActivatorProfile", menuName = "Profiles/Gadgets/Auto Activator Profile")]
public class AutoActivatorProfile : BaseActivatorProfile 
{
    [Header("Auto-Activation Settings")]
    [Tooltip("How often (in seconds) an AutoActivator should trigger its target.")]
    public float ActivationInterval = 1f;
}