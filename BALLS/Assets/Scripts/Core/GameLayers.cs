// Filename: GameLayers.cs
using UnityEngine;

namespace Core
{
    /// <summary>
    /// A static class to hold references to project-specific physics layers.
    /// This avoids using "magic numbers" (hardcoded integers) for layer masks.
    /// 
    /// IMPORTANT: You must create corresponding layers in Unity's Tag and Layer Manager
    /// for these values to be valid.
    /// </summary>
    public static class GameLayers
    {
        public static readonly int Default = LayerMask.NameToLayer("Default");

        /// <summary>
        /// The layer for standard, active balls.
        /// </summary>
        public static readonly int Ball = LayerMask.NameToLayer("Ball");

        /// <summary>
        /// A special layer for balls that are currently in the process of merging.
        /// </summary>
        public static readonly int MergingBall = LayerMask.NameToLayer("MergingBall");
    }
}