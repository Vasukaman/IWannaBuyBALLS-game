// Filename: BaseActivatorProfile.cs
using UnityEngine;

// We don't add a CreateAssetMenu here because we never want to create just a "base" profile.
public class BaseActivatorProfile : ScriptableObject
{
    [Header("Shared Settings")]
    [Tooltip("How often (in seconds) the activator should rescan for the nearest target.")]
    public float TargetScanInterval = 0.5f;
}