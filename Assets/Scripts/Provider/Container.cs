using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Game;
using Utilities;
using VContainer;
using VContainer.Unity;
using View;
using View.UI;

namespace Provider
{
    internal class Container : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameManager>(Lifetime.Singleton);
            builder.Register<World>(Lifetime.Singleton);
            builder.Register<InputReceiver>(Lifetime.Singleton);
            builder.Register<GameInput>(Lifetime.Singleton);
            builder.Register<EffectViewSpawner>(Lifetime.Singleton);
            builder.Register<ChoiceReceiver>(Lifetime.Singleton);
            builder.Register<CharacterSelectReceiver>(Lifetime.Singleton);
            builder.Register<TextInputReceiver>(Lifetime.Singleton);
            builder.Register<CharacterControlInputReceiver>(Lifetime.Singleton);
            builder.Register<SynchronizedItemView>(Lifetime.Singleton);
            builder.Register<SynchronizedCharacterView>(Lifetime.Singleton);
            builder.Register<SynchronizedIconEntityView>(Lifetime.Singleton);
            builder.Register<SynchronizedThrowAnimationEntityView>(Lifetime.Singleton);
            builder.Register<SynchronizedFireEntityView>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<DungeonInfoView>();
            builder.RegisterComponentInHierarchy<TilePalette>();
            builder.RegisterComponentInHierarchy<TileViewController>();
            builder.RegisterComponentInHierarchy<OverlayTileViewController>();
            builder.RegisterComponentInHierarchy<MinimapController>();
            builder.RegisterComponentInHierarchy<InventoryView>();
            builder.RegisterComponentInHierarchy<StatView>();
            builder.RegisterComponentInHierarchy<CameraFollowTarget>();
            builder.RegisterComponentInHierarchy<CameraFlameRect>();
            builder.RegisterComponentInHierarchy<MainMenu>();
            builder.RegisterComponentInHierarchy<SettingWindow>();
            builder.RegisterComponentInHierarchy<MenuController>();
            builder.RegisterComponentInHierarchy<LogView>();
            builder.RegisterComponentInHierarchy<ShopInfoView>();
            builder.RegisterComponentInHierarchy<ItemSelectText>();
            builder.RegisterComponentInHierarchy<DamageTextSpawner>();
            builder.RegisterComponentInHierarchy<FlushController>();
            builder.RegisterComponentInHierarchy<BGMManager>();
            builder.RegisterComponentInHierarchy<SEManager>();
            builder.RegisterComponentInHierarchy<ItemLibraryView>();

            builder.RegisterPlainEntryPoint<InitPresenter>();
            builder.RegisterPlainEntryPoint<DungeonInfoPresenter>();
            builder.RegisterPlainEntryPoint<InputPresenter>();
            builder.RegisterPlainEntryPoint<TilemapPresenter>();
            builder.RegisterPlainEntryPoint<PlayerPresenter>();
            builder.RegisterPlainEntryPoint<PlayerInventoryPresenter>();
            builder.RegisterPlainEntryPoint<PlayerCameraPresenter>();
            builder.RegisterPlainEntryPoint<EffectPreviewPresenter>();
            builder.RegisterPlainEntryPoint<DamagePresenter>();
            builder.RegisterPlainEntryPoint<SoundPresenter>();
            builder.RegisterPlainEntryPoint<GroupMarkerPresenter>();
            builder.RegisterPlainEntryPoint<KeyCharacterPresenter>();
            builder.RegisterPlainEntryPoint<MainMenuPresenter>();
            builder.RegisterPlainEntryPoint<SettingPresenter>();
            builder.RegisterPlainEntryPoint<LogPresenter>();
            builder.RegisterPlainEntryPoint<ShopInfoPresenter>();
            builder.RegisterPlainEntryPoint<StatisticsPresenter>();
            builder.RegisterPlainEntryPoint<ItemSelectPresenter>();
            builder.RegisterPlainEntryPoint<Presenter>();

            builder.RegisterPlainEntryPoint<DebugCommands>();
            builder.RegisterPlainEntryPoint<LogCommands>();
            builder.RegisterPlainEntryPoint<CharacterCommands>();
            builder.RegisterPlainEntryPoint<ItemCommands>();
            builder.RegisterPlainEntryPoint<SpawnCommands>();
            builder.RegisterPlainEntryPoint<MapCommands>();
        }
    }
}