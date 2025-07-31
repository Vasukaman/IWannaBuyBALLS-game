using Core.Spawning;
using Game.Economy;
using Gameplay.BallSystem;
using Gameplay.Gadgets;
using Reflex.Core;
using Services.Ball;
using Services.Gadgets;
using Services.Store;
using UnityEngine;
using UnityEngine.UIElements;
public class ProjectInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private GadgetLibrary _gadgetLibrary;
  //  [SerializeField] private PrefabInstantiator _prefabInstantiator;

    [Header("Prefab Blueprints")]
    [SerializeField] private BallView _ballPrefab; // Add this reference

    [Header("Game Configuration")]
    [SerializeField] private int _initialBallPoolSize = 20; // Add this setting
    public void InstallBindings(ContainerBuilder builder)
    {
        Debug.Log("[ProjectInstaller] InstallBindings running!");

        // Register a literal value
        //IBallFactory fballFactory = GameObject
        //        .FindObjectOfType<BallFactory>(true)  // search scene for your BallFactory
        //        as IBallFactory; 

        //builder.AddSingleton<IBallFactory>(ctr => fballFactory);

        //IGadgetFactory fgadgetFactory = GameObject
        //.FindObjectOfType<GadgetFactory>(true)  // search scene for your BallFactory
        //as IGadgetFactory;

      //  builder.AddSingleton<IGadgetFactory>(ctr => fgadgetFactory);

        IButton fbutton = GameObject
        .FindObjectOfType<ClickableButton>(true)  // search scene for your BallFactory
        as IButton;

        builder.AddSingleton<IButton>(ctr => fbutton);

        MoneyService moneyService = new MoneyService();
        builder.AddSingleton<IMoneyService>(ctr => moneyService);


        StoreService storeService = new StoreService(moneyService, _gadgetLibrary);
        builder.AddSingleton<IStoreService>(ctr => storeService);

        IPrefabInstantiator _prefabInstantiator = GameObject
                .FindObjectOfType<PrefabInstantiator>(true)  // search scene for your BallFactory
                as IPrefabInstantiator;

        builder.AddSingleton<IPrefabInstantiator>(ctr => _prefabInstantiator);

        // 2. Create the Gadget Service instance
        // We pass it the dependencies it needs: the instantiator, the store service, and the container itself.
        GadgetService gadgetService = new GadgetService(_prefabInstantiator, storeService, Container.ProjectContainer);
        builder.AddSingleton<IGadgetService>(ctr => gadgetService);

        // We pass it all the dependencies it needs from its constructor.
        BallService ballService = new BallService(
            _prefabInstantiator,
            Container.ProjectContainer,
            _ballPrefab,
            _initialBallPoolSize
        );

        // 2. Bind the instance to the IBallService interface.
        builder.AddSingleton<IBallService>(ctr => ballService);




    }
}
