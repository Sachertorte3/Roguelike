#nullable enable
using System;
using BidirectionalMap;
using Codice.Client.Commands;
using Domain.Model.Setting;
using Domain.Service.Entities;
using Domain.Service.Events;
using Model.Game;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using Utilities.ObjectsManager;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedEventEntityView : SynchronizedEntityView<IEventEntityAndIcon, EntityView>, IDisposable
    {
        private readonly SerialDisposable _disposable = new();
        protected override InputReceiver _inputReceiver { get; init; }
        protected override EntityView GetEntityView(EntityView view) => view;

        [Inject]
        public SynchronizedEventEntityView(World world, InputReceiver inputReceiver)
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

        ~SynchronizedEventEntityView()
        {
            Dispose();
        }

        protected override void InitializeView(IEventEntityAndIcon eventEntity, EntityView entityView)
        {
            var spriteView = entityView.GetComponent<SpriteView>();
            spriteView.GetComponent<SpriteRenderer>().sprite = eventEntity.Icon;
        }

        protected override void CleanupView(IEventEntityAndIcon item, EntityView view)
        {
        }
    }
}