using Assets.Scripts.View;
using RandomDungeonWithBluePrint;
using Scripts.View;
using Scripts.View.UI;
using UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scripts.Provider
{
    internal class Container : LifetimeScope
    {
        [SerializeField] private FieldBluePrint _bluePrint;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<InputReceiver>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<TileViewContriller>();
            builder.RegisterComponent(_bluePrint);
            builder.RegisterComponentInHierarchy<CameraFollowTarget>();
            builder.RegisterComponentInHierarchy<SettingWindow>();
            builder.RegisterComponentInHierarchy<MenuController>();
            builder.RegisterEntryPoint<Presenter>(Lifetime.Scoped);
            builder.RegisterEntryPoint<SettingPresenter>(Lifetime.Scoped);
        }
    }
}
