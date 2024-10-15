using Domain.Model.Dungeon;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Game;
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
        [SerializeField] private DungeonBluePrintData _dungeonData;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameManager>(Lifetime.Singleton);
            builder.Register<World>(Lifetime.Singleton);
            builder.Register<InputReceiver>(Lifetime.Singleton);
            builder.Register<GameInput>(Lifetime.Singleton);
            builder.Register<EffectViewSpawner>(Lifetime.Singleton);
            builder.Register<ChoiceReceiver>(Lifetime.Singleton);
            builder.Register<TextInputReceiver>(Lifetime.Singleton);
            builder.Register<CharacterControlInputReceiver>(Lifetime.Singleton);
            builder.Register<SynchronizedItemView>(Lifetime.Singleton);
            builder.Register<SynchronizedCharacterView>(Lifetime.Singleton);
            builder.Register<SynchronizedIconEntityView>(Lifetime.Singleton);
            builder.Register<SynchronizedThrowAnimationEntityView>(Lifetime.Singleton);
            builder.RegisterComponent(_dungeonData);
            builder.RegisterComponentInHierarchy<DungeonInfoView>();
            builder.RegisterComponentInHierarchy<TileViewController>();
            builder.RegisterComponentInHierarchy<OverlayTileViewController>();
            builder.RegisterComponentInHierarchy<InventoryView>();
            builder.RegisterComponentInHierarchy<StatLine>();
            builder.RegisterComponentInHierarchy<CameraFollowTarget>();
            builder.RegisterComponentInHierarchy<SettingWindow>();
            builder.RegisterComponentInHierarchy<MenuController>();
            builder.RegisterComponentInHierarchy<LogView>();
            builder.RegisterComponentInHierarchy<ShopInfoView>();
            builder.RegisterComponentInHierarchy<ItemSelectText>();
            builder.RegisterComponentInHierarchy<DamageTextSpawner>();
            builder.RegisterComponentInHierarchy<FlushController>();
            builder.RegisterComponentInHierarchy<BGMManager>();
            builder.RegisterComponentInHierarchy<SEManager>();

            builder.RegisterPlainEntryPoint<DungeonInfoPresenter>();
            builder.RegisterPlainEntryPoint<InputPresenter>();
            builder.RegisterPlainEntryPoint<TilemapPresenter>();
            builder.RegisterPlainEntryPoint<PlayerPresenter>();
            builder.RegisterPlainEntryPoint<PlayerInventoryPresenter>();
            builder.RegisterPlainEntryPoint<PlayerCameraController>();
            builder.RegisterPlainEntryPoint<EffectPreviewPresenter>();
            builder.RegisterPlainEntryPoint<DamagePresenter>();
            builder.RegisterPlainEntryPoint<SoundPresenter>();
            builder.RegisterPlainEntryPoint<GroupMarkerPresenter>();
            builder.RegisterPlainEntryPoint<KeyCharacterPresenter>();
            builder.RegisterPlainEntryPoint<SettingPresenter>();
            builder.RegisterPlainEntryPoint<LogPresenter>();
            builder.RegisterPlainEntryPoint<ShopInfoPresenter>();
            builder.RegisterPlainEntryPoint<ItemSelectPresenter>();
            builder.RegisterPlainEntryPoint<Presenter>();
            builder.RegisterPlainEntryPoint<DebugCommands>();
        }
    }
}