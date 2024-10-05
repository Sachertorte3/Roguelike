#nullable enable
using System;
using System.Linq;
using Domain.Model;
using Domain.Model.Setting;
using Game;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedItemView : SynchronizedEntityView<IItemEntity, EntityView>, IDisposable
    {
        private readonly SerialDisposable _disposable = new();
        private readonly EffectViewSpawner _effectViewSpawner;
        protected override InputReceiver _inputReceiver { get; init; }
        private readonly World _world;

        protected override EntityView GetEntityView(EntityView view)
        {
            return view;
        }

        [Inject]
        public SynchronizedItemView(World world, EffectViewSpawner effectViewSpawner, InputReceiver inputReceiver)
        {
            _effectViewSpawner = effectViewSpawner;
            _inputReceiver = inputReceiver;
            _world = world;

            world.ActiveMap.SubscribeToAllIgnoreNull(
                map => _disposable.Disposable = map.Items.SubscribeToAll(Add, Remove),
                map => map.Items.ForEach(item => Remove(item))
            );
        }

        protected override EntityView ViewPrefab(IItemEntity _)
        {
            return Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/ItemView.prefab").WaitForCompletion()
                .GetComponent<EntityView>();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        ~SynchronizedItemView()
        {
            Dispose();
        }

        protected override void InitializeView(IItemEntity item, EntityView entityView)
        {
            item.OnEffectSpawned.Subscribe(useSkill =>
                    _effectViewSpawner.Spawn(useSkill.Area.Intersect(_world.ActiveMap.CurrentValue.VisibleArea),
                        useSkill.Color, Settings.EffectDisplayTime.Value))
                .AddTo(entityView);

            var spriteView = entityView.GetComponent<SpriteView>();
            spriteView.GetComponent<SpriteRenderer>().sprite = item.Icon;
        }

        protected override void CleanupView(IItemEntity item, EntityView view)
        {
        }
    }
}