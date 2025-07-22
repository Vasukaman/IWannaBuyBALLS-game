using Game.Economy;
using Reflex.Core;
using UnityEngine;
using UnityEngine.UIElements;

public class ProjectInstaller : MonoBehaviour, IInstaller
{

    public void InstallBindings(ContainerBuilder builder)
    {
        Debug.Log("[ProjectInstaller] InstallBindings running!");

        // Register a literal value
        IBallFactory fballFactory = GameObject
                .FindObjectOfType<BallFactory>(true)  // search scene for your BallFactory
                as IBallFactory; 

        builder.AddSingleton<IBallFactory>(ctr => fballFactory);

        IGadgetFactory fgadgetFactory = GameObject
        .FindObjectOfType<GadgetFactory>(true)  // search scene for your BallFactory
        as IGadgetFactory;

        builder.AddSingleton<IGadgetFactory>(ctr => fgadgetFactory);

        IButton fbutton = GameObject
        .FindObjectOfType<ClickableButton>(true)  // search scene for your BallFactory
        as IButton;

        builder.AddSingleton<IButton>(ctr => fbutton);

        MoneyService moneyService = new MoneyService();
        builder.AddSingleton<IMoneyService>(ctr => moneyService);
    }
}
