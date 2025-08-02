// Filename: ITargetableVisual.cs
using UnityEngine;

namespace VFX.Interfaces
{
    /// <summary>
    /// Represents any visual effect component that can be aimed at a target Transform.
    /// This allows a Presenter to control any visual effect without knowing its specific type.
    /// </summary>
    public interface ITargetableVisual
    {
        Transform Target { get; set; }
    }
}