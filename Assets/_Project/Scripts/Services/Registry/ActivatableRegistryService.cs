// Filename: ActivatableRegistryService.cs
using System.Collections.Generic;

namespace Services.Registry
{
    public class ActivatableRegistryService : IActivatableRegistry
    {
        private readonly List<IActivatable> _activatables = new List<IActivatable>();
        public IReadOnlyList<IActivatable> AllActivatables => _activatables;

        public void Register(IActivatable activatable)
        {
            if (!_activatables.Contains(activatable))
            {
                _activatables.Add(activatable);
            }
        }

        public void Unregister(IActivatable activatable)
        {
            _activatables.Remove(activatable);
        }
    }
}