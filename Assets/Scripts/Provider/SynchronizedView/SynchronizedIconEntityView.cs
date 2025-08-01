#nullable enable
using System;
using System.Linq;
using Domain.Model.Entity;
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
    public class SynchronizedIconEntityView : SynchronizedEntityView<IEntity, EntityView>, IDisposable
    {
        private readonly SerialDisposable[] _disposable =
            EnumerableExtension.CreateNewInstances<SerialDisposable>(2).ToArray();

        protected override InputReceiver _inputReceiver { get; init; }

        protected override EntityView GetEntityView(EntityView view)
        {
            return view;
        }

        [Inject]
        public SynchronizedIconEntityView(World world, InputReceiver inputReceiver)
        {
            _inputReceiver = inputReceiver;

            world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(
                map => _disposable[0].Disposable =
                    map.EventEntityManager.StandaloneEventEntities.SubscribeIncludingCurrentItems(Add, Remove),
                map => map.EventEntityManager.StandaloneEventEntities.ForEach(entity => Remove(entity))
            );
            world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(
                map => _disposable[1].Disposable =
                    map.EventEntityManager.StandalonePlayerEventEntities.SubscribeIncludingCurrentItems(Add, Remove),
                map => map.EventEntityManager.StandalonePlayerEventEntities.ForEach(entity => Remove(entity))
            );
        }

        protected override EntityView ViewPrefab(IEntity eventEntity)
        {
            if (eventEntity is Bonfire)
            {
                return ScriptableObjectLoader.LoadPrefab("Bonfire").GetComponent<EntityView>();
            }

            if (eventEntity is Money)
            {
                return ScriptableObjectLoader.LoadPrefab("Money").GetComponent<EntityView>();
            }

            if (eventEntity is Trap)
            {
                return ScriptableObjectLoader.LoadPrefab("Trap").GetComponent<EntityView>();
            }

            if (eventEntity is Stairs stairs)
            {
                switch (stairs.Type)
                {
                    case MovementEntityType.UpStairs:
                        return ScriptableObjectLoader.LoadPrefab("UpStairs").GetComponent<EntityView>();
                    case MovementEntityType.DownStairs:
                        return ScriptableObjectLoader.LoadPrefab("DownStairs").GetComponent<EntityView>();
                    case MovementEntityType.MagicCircle:
                        return ScriptableObjectLoader.LoadPrefab("MagicCircle").GetComponent<EntityView>();
                    default:
                        throw new NotImplementedException();
                }
            }

            if (eventEntity.Entity.Layer == EntityLayer.Middle)
            {
                return ScriptableObjectLoader.LoadPrefab("Entity").GetComponent<EntityView>();
            }

            return ScriptableObjectLoader.LoadPrefab("EntityBottom").GetComponent<EntityView>();
        }

        public void Dispose()
        {
            foreach (var disposable in _disposable)
            {
                disposable.Dispose();
            }
        }

        ~SynchronizedIconEntityView()
        {
            Dispose();
        }

        protected override void InitializeView(IEntity eventEntity, EntityView entityView)
        {
            var spriteView = entityView.GetComponent<SpriteView>();
            if (eventEntity is IIconEntity iconEventEntity)
                spriteView.GetComponent<SpriteRenderer>().sprite = iconEventEntity.Icon;
        }

        protected override void CleanupView(IEntity item, EntityView view)
        {
        }
    }
}