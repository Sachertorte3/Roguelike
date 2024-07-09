#nullable enable
using System;
using Domain.Model;
using Model.Game;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedIconEntityView : SynchronizedEntityView<IIconEntity, EntityView>, IDisposable
    {
        private readonly SerialDisposable _disposable = new();
        protected override InputReceiver _inputReceiver { get; init; }
        protected override EntityView GetEntityView(EntityView view) => view;

        [Inject]
        public SynchronizedIconEntityView(World world, InputReceiver inputReceiver)
        {
            _inputReceiver = inputReceiver;

            world.ActiveMap.SubscribeToAllIgnoreNull(
                map => _disposable.Disposable = map.EventEntitiesAndIcons.SubscribeToAll(Add, Remove),
                map => map.EventEntitiesAndIcons.ForEach(entity => Remove(entity))
            );
        }

        protected override EntityView _viewPrefab =>
            Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Stairs.prefab").WaitForCompletion()
                .GetComponent<EntityView>();

        public void Dispose()
        {
            _disposable.Dispose();
        }

        ~SynchronizedIconEntityView()
        {
            Dispose();
        }

        protected override void InitializeView(IIconEntity eventEntity, EntityView entityView)
        {
            var spriteView = entityView.GetComponent<SpriteView>();
            spriteView.GetComponent<SpriteRenderer>().sprite = eventEntity.Icon;
        }

        protected override void CleanupView(IIconEntity item, EntityView view)
        {
        }
    }
}