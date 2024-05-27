#nullable enable
using Model;
using R3;
using System.Collections.Generic;
using System.Linq;
using Data.Setting;
using Model.Domain.Items;
using Model.Game;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using Utilities.ObjectsManager;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedItemView : SynchronizedView<ItemEntity, EntityView>
    {
        private readonly EffectViewSpawner _effectViewSpawner;
        private readonly InputReceiver _inputReceiver;
        private readonly IReadOnlyCollection<Vector2Int> _visibleArea;

        [Inject]
        public SynchronizedItemView(World world, EffectViewSpawner effectViewSpawner, InputReceiver inputReceiver)
        {
            _effectViewSpawner = effectViewSpawner;
            _inputReceiver = inputReceiver;
            _visibleArea = world.VisibleArea;

            world.Items.Set.SubscribeToAll(Add, Remove);
        }

        protected override EntityView _viewPrefab =>
            Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/ItemView.prefab").WaitForCompletion()
                .GetComponent<EntityView>();

        protected override void InitializeView(ItemEntity item, EntityView entityView)
        {
            entityView.Construct(_inputReceiver);
            item.OnMove.Subscribe(move => entityView.Move(move.destination, move.direction));
            item.OnTeleport.Subscribe(teleport => entityView.Teleport(teleport)).AddTo(entityView);
            item.OnSpawnEffect.Subscribe(useSkill =>
                    _effectViewSpawner.Spawn(useSkill.Intersect(_visibleArea), Settings.EffectDisplayTime.Value))
                .AddTo(entityView);
            Settings.ThrowMilliseconds.Subscribe(value => entityView.MoveMilliseconds = value).AddTo(entityView);
            Settings.ThrowMilliseconds.Subscribe(value => entityView.DashMilliseconds = value).AddTo(entityView);

            var spriteView = entityView.GetComponent<SpriteView>();
            spriteView.RegisterComponent();
            spriteView.transform.position = (Vector3Int)item.CurrentPosition;
            spriteView.GetComponent<SpriteRenderer>().sprite = item.Icon;
            item.Visibility.Subscribe(visibility => spriteView.SetVisibility(visibility)).AddTo(spriteView);
        }

        protected override void CleanupView(ItemEntity item, EntityView view)
        {
        }
    }
}