#nullable enable
using System;
using Domain.Model;
using Domain.Model.Map;
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
    public class SynchronizedIconEntityView : SynchronizedEntityView<IEventEntity, EntityView>, IDisposable
    {
        private readonly SerialDisposable _disposable = new();
        protected override InputReceiver _inputReceiver { get; init; }

        protected override EntityView GetEntityView(EntityView view)
        {
            return view;
        }

        [Inject]
        public SynchronizedIconEntityView(World world, InputReceiver inputReceiver)
        {
            _inputReceiver = inputReceiver;

            world.ActiveMap.SubscribeToAllIgnoreNull(
                map => _disposable.Disposable =
                    map.EventEntityManager.StandaloneEventEntities.SubscribeToAll(Add, Remove),
                map => map.EventEntityManager.StandaloneEventEntities.ForEach(entity => Remove(entity))
            );
        }

        protected override EntityView ViewPrefab(IEventEntity eventEntity)
        {
            if (eventEntity is Bonfire)
            {
                return Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Bonfire.prefab").WaitForCompletion()
                    .GetComponent<EntityView>();
            }
            else if (eventEntity is Money)
            {
                return Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Money.prefab").WaitForCompletion()
                    .GetComponent<EntityView>();
            }
            else if (eventEntity is Trap)
            {
                return Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Trap.prefab").WaitForCompletion()
                    .GetComponent<EntityView>();
            }
            else if (eventEntity is Stairs stairs)
            {
                switch (stairs.Type)
                {
                    case MovementEntityType.UpStairs:
                        return Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/UpStairs.prefab").WaitForCompletion()
                            .GetComponent<EntityView>();
                    case MovementEntityType.DownStairs:
                        return Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/DownStairs.prefab").WaitForCompletion()
                            .GetComponent<EntityView>();
                    case MovementEntityType.MagicCircle:
                        return Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/MagicCircle.prefab").WaitForCompletion()
                            .GetComponent<EntityView>();
                    default:
                        throw new NotImplementedException();
                }
            }

            if (eventEntity.Layer == EntityLayer.Middle)
            {
                return Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Entity.prefab").WaitForCompletion()
                    .GetComponent<EntityView>();
            }

            return Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/EntityBottom.prefab").WaitForCompletion()
                .GetComponent<EntityView>();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        ~SynchronizedIconEntityView()
        {
            Dispose();
        }

        protected override void InitializeView(IEventEntity eventEntity, EntityView entityView)
        {
            var spriteView = entityView.GetComponent<SpriteView>();
            if (eventEntity is IIconEntity iconEventEntity)
                spriteView.GetComponent<SpriteRenderer>().sprite = iconEventEntity.Icon;
        }

        protected override void CleanupView(IEventEntity item, EntityView view)
        {
        }
    }
}