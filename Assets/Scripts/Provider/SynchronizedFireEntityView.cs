#nullable enable
using System;
using Domain.Service.Events;
using Game;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedFireEntityView : SynchronizedEntityView<Fire, EntityView>,
        IDisposable
    {
        private readonly SerialDisposable _disposable = new();
        protected override InputReceiver _inputReceiver { get; init; }

        protected override EntityView GetEntityView(EntityView view)
        {
            return view;
        }

        [Inject]
        public SynchronizedFireEntityView(World world, InputReceiver inputReceiver)
        {
            _inputReceiver = inputReceiver;

            world.ActiveMap.SubscribeToAllItemsIgnoreNull(
                map => _disposable.Disposable = map.FireEntities.SubscribeToAllItems(Add, Remove),
                map => map.FireEntities.ForEach(entity => Remove(entity))
            );
        }

        protected override EntityView ViewPrefab(Fire _)
        {
            return Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Fire.prefab").WaitForCompletion()
                .GetComponent<EntityView>();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        ~SynchronizedFireEntityView()
        {
            Dispose();
        }

        protected override void InitializeView(Fire eventEntity, EntityView entityView)
        {
        }

        protected override void CleanupView(Fire item, EntityView view)
        {
        }
    }
}