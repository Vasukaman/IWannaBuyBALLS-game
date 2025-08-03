// Filename: GadgetService.cs
using Core.Spawning;
using Gameplay.Gadgets;
using Reflex.Core;
using Reflex.Injectors;
using Services.Store;
using UnityEngine;

namespace Services.Gadgets
{
    public class GadgetService : IGadgetService
    {
        private readonly IPrefabInstantiator _instantiator;
        private readonly IStoreService _storeService;
        private readonly Container _container;

        public GadgetService(IPrefabInstantiator instantiator, IStoreService storeService, Container container)
        {
            _instantiator = instantiator;
            _storeService = storeService;
            _container = container;
        }

        // Implement the updated method signature
        public PlaceableView CreateGadget(GadgetData data, Vector3 position, Transform parent = null)
        {
            if (data.Prefab == null)
            {
                Debug.LogError($"Gadget prefab for '{data.DisplayName}' is null!");
                return null;
            }

            // 1. CREATE THE BODY - Pass the parent transform to the instantiator
            GameObject instance = _instantiator.InstantiatePrefab(data.Prefab, position, parent);
            PlaceableView view = instance.GetComponent<PlaceableView>();

            // 2. CREATE THE BRAIN
            PlaceableModel model = new PlaceableModel(data, _storeService);

            // 3. CONNECT THEM
            view.Initialize(model);

            // 4. INJECT DEPENDENCIES
            GameObjectInjector.InjectRecursive(instance, Container.ProjectContainer);


            return view;
        }
    }
}