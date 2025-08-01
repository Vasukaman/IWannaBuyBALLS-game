// Filename: IActivatableRegistry.cs
using System.Collections.Generic;

namespace Services.Registry
{
    public interface IActivatableRegistry
    {
        IReadOnlyList<IActivatable> AllActivatables { get; }
        void Register(IActivatable activatable);
        void Unregister(IActivatable activatable);
    }
}