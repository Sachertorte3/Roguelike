using Domain.Model;
using Domain.Service.Characters.Behavior;
using Model.Game;
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
        [SerializeField] private DungeonData _dungeonData;

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
            builder.Register<SynchronizedIconEntityView>(Lifetime.Singleton);
            builder.RegisterComponent(_dungeonData);
            builder.RegisterComponentInHierarchy<TileViewController>();
            builder.RegisterComponentInHierarchy<TileMaskController>();
            builder.RegisterComponentInHierarchy<InventoryView>();
            builder.RegisterComponentInHierarchy<StatLine>();
            builder.RegisterComponentInHierarchy<CameraFollowTarget>();
            builder.RegisterComponentInHierarchy<SettingWindow>();
            builder.RegisterComponentInHierarchy<MenuController>();
            builder.RegisterComponentInHierarchy<LogView>();
            builder.RegisterComponentInHierarchy<ShopInfoView>();
            builder.RegisterComponentInHierarchy<DamageTextSpawner>();
            builder.RegisterComponentInHierarchy<FlushController>();
            builder.RegisterComponentInHierarchy<BGMManager>();
            builder.RegisterComponentInHierarchy<SEManager>();

            builder.RegisterPlainEntryPoint<InputPresenter>();
            builder.RegisterPlainEntryPoint<TilemapPresenter>();
            builder.RegisterPlainEntryPoint<PlayerPresenter>();
            builder.RegisterPlainEntryPoint<PlayerInventoryPresenter>();
            builder.RegisterPlainEntryPoint<PlayerCameraController>();
            builder.RegisterPlainEntryPoint<DamagePresenter>();
            builder.RegisterPlainEntryPoint<SoundPresenter>();
            builder.RegisterPlainEntryPoint<GroupMarkerPresenter>();
            builder.RegisterPlainEntryPoint<SettingPresenter>();
            builder.RegisterPlainEntryPoint<LogPresenter>();
            builder.RegisterPlainEntryPoint<ShopInfoPresenter>();
            builder.RegisterPlainEntryPoint<Presenter>();
        }
    }
}