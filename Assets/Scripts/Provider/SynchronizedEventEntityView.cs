#nullable enable
using System;
using System.Linq;
using Domain.Model;
using Domain.Service.Events;
using Domain.Service.Items;
using Domain.Service.Rooms;
using Game;
using R3;
using Sirenix.OdinInspector.Editor.Drawers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TextCore.Text;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedIconEntityView : SynchronizedEntityView<IEventEntity, EntityView>, IDisposable
    {
        private readonly SerialDisposable _disposable = new();
        protected override InputReceiver _inputReceiver { get; init; }
        protected override EntityView GetEntityView(EntityView view) => view;

        [Inject]
        public SynchronizedIconEntityView(World world, InputReceiver inputReceiver)
        {
            _inputReceiver = inputReceiver;

            world.ActiveMap.SubscribeToAllIgnoreNull(
                map => _disposable.Disposable = map.EventEntityManager.StandaloneEventEntities.SubscribeToAll(Add, Remove),
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
            else
            {
                return Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Stairs.prefab").WaitForCompletion()
                    .GetComponent<EntityView>();
            }
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