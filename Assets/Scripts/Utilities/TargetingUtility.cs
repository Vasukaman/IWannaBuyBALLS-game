// Filename: TargetingUtility.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Utilities
{
    /// <summary>
    /// A static helper class for common targeting logic.
    /// </summary>
    public static class TargetingUtility
    {
        public static IActivatable FindNearestTarget(Transform owner, IReadOnlyList<IActivatable> allTargets)
        {
            if (allTargets.Count == 0)
            {
                return null;
            }

            return allTargets
                .Where(a => a.ActivationTransform != owner)
                .OrderBy(a => Vector3.Distance(owner.position, a.ActivationTransform.position))
                .FirstOrDefault();
        }
    }
}