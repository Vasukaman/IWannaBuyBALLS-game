using UnityEngine;

public interface IActivatable
{
    /// <summary>
    /// Trigger whatever action this object is responsible for.
    /// </summary>
    void Activate();

    /// <summary>
    /// Where in world‐space this activatable “lives.”
    /// </summary>
    Transform ActivationTransform { get; }
}
    