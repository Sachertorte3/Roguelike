using Model;
using Model.Domain.Characters.Behavior;
using Model.Game;
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
            builder.Register<GameInput>(Lifetime.Singleton);
            builder.Register<EffectViewSpawner>(Lifetime.Singleton);
            builder.Register<CharacterControllInputReceiver>(Lifetime.Singleton);
            builder.Register<SynchronizedItemView>(Lifetime.Singleton);
            builder.Register<SynchronizedCharacterView>(Lifetime.Singleton);
            builder.Register<SynchronizedEventEntityView>(Lifetime.Singleton);
            builder.RegisterComponent(_bluePrint);
            builder.RegisterComponentInHierarchy<TileViewController>();
            builder.RegisterComponentInHierarchy<TileMaskController>();
            builder.RegisterComponentInHierarchy<InventoryView>();
            builder.RegisterComponentInHierarchy<StatLine>();
            builder.RegisterComponentInHierarchy<CameraFollowTarget>();
            builder.RegisterComponentInHierarchy<SettingWindow>();
            builder.RegisterComponentInHierarchy<MenuController>();
            builder.RegisterComponentInHierarchy<LogView>();
            builder.RegisterComponentInHierarchy<DamageTextSpawner>();
            builder.RegisterComponentInHierarchy<FlushController>();

            builder.RegisterPlainEntryPoint<InputPresenter>();
            builder.RegisterPlainEntryPoint<TilemapPresenter>();
            builder.RegisterPlainEntryPoint<PlayerPresenter>();
            builder.RegisterPlainEntryPoint<PlayerInventoryPresenter>();
            builder.RegisterPlainEntryPoint<PlayerCameraController>();
            builder.RegisterPlainEntryPoint<DamagePresenter>();
            builder.RegisterPlainEntryPoint<SettingPresenter>();
            builder.RegisterPlainEntryPoint<LogPresenter>();
            builder.RegisterPlainEntryPoint<Presenter>();
        }
    }
}