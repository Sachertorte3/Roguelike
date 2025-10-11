#nullable enable
using System;
using System.Linq;
using DG.Tweening;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Service.Events;
using Game;
using R3;
using UnityEngine;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedIconEntityView : SynchronizedEntityView<IEntity, EntityView>, IDisposable
    {
        private readonly SerialDisposable[] _disposable =
            EnumerableExtension.CreateNewInstances<SerialDisposable>(3).ToArray();

        protected override InputReceiver _inputReceiver { get; init; }
        protected override GameManager _gameManager { get; init; }
        protected override World _world { get; init; }

        protected override EntityView GetEntityView(EntityView view)
        {
            return view;
        }

        [Inject]
        public SynchronizedIconEntityView(World world, InputReceiver inputReceiver, GameManager gameManager)
        {
            _inputReceiver = inputReceiver;
            _gameManager = gameManager;
            _world = world;

            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                mapChanged.PreviousMap?.StandaloneEventEntities.ForEach(entity => Remove(entity));
                _disposable[0].Disposable = mapChanged.Map.StandaloneEventEntities.SubscribeIncludingCurrentItems(Add, Remove);
            });
            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                mapChanged.PreviousMap?.StandalonePlayerEventEntities.ForEach(entity => Remove(entity));
                _disposable[1].Disposable = mapChanged.Map.StandalonePlayerEventEntities.SubscribeIncludingCurrentItems(Add, Remove);
            });
            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                mapChanged.PreviousMap?.StandaloneScheduledEventEntities.ForEach(entity => Remove(entity));
                _disposable[2].Disposable = mapChanged.Map.StandaloneScheduledEventEntities.SubscribeIncludingCurrentItems(Add, Remove);
            });
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

            if (eventEntity is Statue)
            {
                return ScriptableObjectLoader.LoadPrefab("Statue").GetComponent<EntityView>();
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
            else
            {
                return ScriptableObjectLoader.LoadPrefab("EntityBottom").GetComponent<EntityView>();
            }
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
            if (eventEntity is Bonfire bonfire)
            {
                var bonfireView = entityView.GetComponent<BonfireView>();
                bonfire.IsFire
                    .Subscribe(isFire => bonfireView.ShowFire(isFire))
                    .AddTo(entityView);
            }
            else if (eventEntity is MagicPot magicPot)
            {
                magicPot.CanUse
                    .Subscribe(canUse => spriteView.GetComponent<SpriteRenderer>().sprite = magicPot.Icon)
                    .AddTo(entityView);
            }
            else if (eventEntity is Statue statue)
            {
                statue.OnAttacked
                    .Subscribe(_ => entityView.transform.DOShakePosition(0.5f, 0.1f))
                    .AddTo(entityView);
            }
            else if (eventEntity is IIconEntity iconEventEntity)
                spriteView.GetComponent<SpriteRenderer>().sprite = iconEventEntity.Icon;
        }

        protected override void CleanupView(IEntity item, EntityView view)
        {
        }
    }
}