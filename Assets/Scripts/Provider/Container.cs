using RandomDungeonWithBluePrint;
using Scripts.Model;
using Scripts.Model.Characters;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Map;
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
            builder.Register<GameManager>(Lifetime.Singleton);
            builder.Register<Tilemap>(Lifetime.Singleton);
            builder.Register<CharacterManager>(Lifetime.Singleton);
            builder.Register<World>(Lifetime.Singleton);
            builder.Register<InputReceiver>(Lifetime.Singleton);
            builder.Register<EffectViewSpawner>(Lifetime.Singleton);
            builder.Register<CharacterControllInputReceiver>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<TileViewController>();
            builder.RegisterComponentInHierarchy<TileMaskController>();
            builder.RegisterComponent(_bluePrint);
            builder.RegisterComponentInHierarchy<CameraFollowTarget>();
            builder.RegisterComponentInHierarchy<SettingWindow>();
            builder.RegisterComponentInHierarchy<MenuController>();
            builder.Register<VisibleArea>(Lifetime.Singleton);
            builder.Register<SynchronizedCharacterView>(Lifetime.Singleton);

            builder.RegisterPlainEntryPoint<InputPresenter>();
            builder.RegisterPlainEntryPoint<TilemapPresenter>();
            builder.RegisterPlainEntryPoint<PlayerPresenter>();
            builder.RegisterPlainEntryPoint<SettingPresenter>();
            builder.RegisterPlainEntryPoint<Presenter>();
        }
    }
}
