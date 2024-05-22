#nullable enable
using Model;
using Model.Setting;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using Utilities.ObjectsManager;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedEventEntityView : SynchronizedView<IEventEntity, EntityView>
    {
        protected override EntityView _viewPrefab =>
            Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Stairs.prefab").WaitForCompletion().GetComponent<EntityView>();

        private readonly InputReceiver _inputReceiver;

        [Inject]
        public SynchronizedEventEntityView(World world, InputReceiver inputReceiver)
        {
            _inputReceiver = inputReceiver;

            world.EventEntities.Set.SubscribeToAll(Add, Remove);
        }

        protected override void InitializeView(IEventEntity eventEntity, EntityView entityView)
        {
            entityView.Construct(_inputReceiver);
            eventEntity.OnMove.Subscribe(move => entityView.Move(move.destination, move.direction));
            eventEntity.OnTeleport.Subscribe(teleport => entityView.Teleport(teleport)).AddTo(entityView);
            Settings.ThrowMilliseconds.Subscribe(value => entityView.MoveMilliseconds = value).AddTo(entityView);
            Settings.ThrowMilliseconds.Subscribe(value => entityView.DashMilliseconds = value).AddTo(entityView);

            var spriteView = entityView.GetComponent<SpriteView>();
            spriteView.RegisterComponent();
            spriteView.transform.position = (Vector3Int)eventEntity.CurrentPosition;
            spriteView.GetComponent<SpriteRenderer>().sprite = eventEntity.Icon;
            eventEntity.Visibility.Subscribe(visibility => spriteView.SetVisibility(visibility)).AddTo(spriteView);
        }

        protected override void CleanupView(IEventEntity item, EntityView view)
        {
        }
    }
}