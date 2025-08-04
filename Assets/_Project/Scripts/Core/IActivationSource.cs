// Filename: IActivationSource.cs
using System;

namespace Gameplay.Interfaces
{
    /// <summary>
    /// Represents any component that can provide an activation signal.
    /// Other components can listen to the OnActivate event without needing to know
    /// the specific type of the activator (e.g., manual vs. automatic).
    /// </summary>
    public interface IActivationSource
    {
        event Action OnActivate;
    }
}