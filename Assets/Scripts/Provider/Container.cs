using Model;
using Model.Characters.Behavior;
using RandomDungeonWithBluePrint;
using UnityEngine;
using Utilities;
using VContainer;
using VContainer.Unity;
using View;
using View.UI;

namespace Provider
{
    internal class Container : LifetimeScope
    {
        [SerializeField] private FieldBluePrint _bluePrint;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameManager>(Lifetime.Singleton);
            builder.Register<World>(Lifetime.Singleton);
            builder.Register<InputReceiver>(Lifetime.Singleton);
            builder.Register<EffectViewSpawner>(Lifetime.Singleton);
            builder.Register<CharacterControllInputReceiver>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<TileViewController>();
            builder.RegisterComponentInHierarchy<TileMaskController>();
            builder.RegisterComponentInHierarchy<InventoryView>();
            builder.RegisterComponentInHierarchy<StatLine>();
            builder.RegisterComponent(_bluePrint);
            builder.RegisterComponentInHierarchy<CameraFollowTarget>();
            builder.RegisterComponentInHierarchy<SettingWindow>();
            builder.RegisterComponentInHierarchy<MenuController>();
            builder.RegisterComponentInHierarchy<LogView>();
            builder.Register<SynchronizedItemView>(Lifetime.Singleton);
            builder.Register<SynchronizedCharacterView>(Lifetime.Singleton);
            builder.Register<SynchronizedEventEntityView>(Lifetime.Singleton);

            builder.RegisterPlainEntryPoint<InputPresenter>();
            builder.RegisterPlainEntryPoint<TilemapPresenter>();
            builder.RegisterPlainEntryPoint<PlayerPresenter>();
            builder.RegisterPlainEntryPoint<PlayerInventoryPresenter>();
            builder.RegisterPlainEntryPoint<PlayerCameraController>();
            builder.RegisterPlainEntryPoint<SettingPresenter>();
            builder.RegisterPlainEntryPoint<LogPresenter>();
            builder.RegisterPlainEntryPoint<Presenter>();
        }
    }
}