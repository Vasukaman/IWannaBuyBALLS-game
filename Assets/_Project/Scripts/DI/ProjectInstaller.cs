using Core.Spawning;
using Services.Money;
using Gameplay.BallSystem;
using Gameplay.Gadgets;
using Reflex.Core;
using Services.Ball;
using Services.Gadgets;
using Services.Store;
using UnityEngine;
using UnityEngine.UIElements;
using Services.Registry;

public class ProjectInstaller : MonoBehaviour, IInstaller

{

    [Header("Data & Asset References")]

    [SerializeField] private EventChannels _eventChannels;

    [SerializeField] private GadgetLibrary _gadgetLibrary;

    [SerializeField] private BallView _ballPrefab;





    [Header("Game Configuration")]

    [SerializeField] private int _initialBallPoolSize = 20; // Add this setting

    public void InstallBindings(ContainerBuilder builder)

    {

        Debug.Log("[ProjectInstaller] InstallBindings running!");


        ActivatableRegistryService _activatableRegistryService = new ActivatableRegistryService();

        builder.AddSingleton<IActivatableRegistry>(ctr => _activatableRegistryService);

        builder.AddSingleton<EventChannels>(ctr => _eventChannels);


        MoneyService moneyService = new MoneyService(_eventChannels);
        builder.AddSingleton<IMoneyService>(ctr => moneyService);



        StoreService storeService = new StoreService(moneyService, _gadgetLibrary);
        builder.AddSingleton<IStoreService>(ctr => storeService);


        //Prob not the best way to do it.
        IPrefabInstantiator _prefabInstantiator = GameObject
            .FindObjectOfType<PrefabInstantiator>(true)  
                    as IPrefabInstantiator;



        builder.AddSingleton<IPrefabInstantiator>(ctr => _prefabInstantiator);

        GadgetService gadgetService = new GadgetService(_prefabInstantiator, storeService, Container.ProjectContainer);
        builder.AddSingleton<IGadgetService>(ctr => gadgetService);


        BallService ballService = new BallService(
                                  _prefabInstantiator,
                                  Container.ProjectContainer,
                                  _ballPrefab,
                                  _initialBallPoolSize
                                );



        builder.AddSingleton<IBallService>(ctr => ballService);


    }

}