#nullable enable
using System;
using Domain.Model.Setting;
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
    public class SynchronizedEventEntityView : SynchronizedView<IEventEntityAndIcon, EntityView>, IDisposable
    {
        private readonly SerialDisposable _disposable = new();
        private readonly InputReceiver _inputReceiver;

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
            entityView.Construct(_inputReceiver);
            eventEntity.OnMove.Subscribe(move => entityView.Move(move.destination, move.direction)).AddTo(entityView);
            eventEntity.OnTeleport.Subscribe(teleport => entityView.Teleport(teleport)).AddTo(entityView);
            Settings.ThrowMilliseconds.Subscribe(value => entityView.SetMoveMilliseconds(value)).AddTo(entityView);
            Settings.ThrowMilliseconds.Subscribe(value => entityView.SetDashMilliseconds(value)).AddTo(entityView);

            var spriteView = entityView.GetComponent<SpriteView>();
            spriteView.RegisterComponent();
            spriteView.transform.position = (Vector3Int)eventEntity.CurrentPosition;
            spriteView.GetComponent<SpriteRenderer>().sprite = eventEntity.Icon;
            eventEntity.Visibility.Subscribe(visibility => spriteView.SetVisibility(visibility)).AddTo(spriteView);
        }

        protected override void CleanupView(IEventEntityAndIcon item, EntityView view)
        {
        }
    }
}