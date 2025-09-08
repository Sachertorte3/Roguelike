#nullable enable
using System;
using Domain.Service.Items;
using Game;
using R3;
using UnityEngine;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedThrowAnimationEntityView : SynchronizedEntityView<ThrowAnimationEntity, EntityView>,
        IDisposable
    {
        private readonly SerialDisposable _disposable = new();
        protected override InputReceiver _inputReceiver { get; init; }

        protected override EntityView GetEntityView(EntityView view)
        {
            return view;
        }

        [Inject]
        public SynchronizedThrowAnimationEntityView(World world, InputReceiver inputReceiver)
        {
            _inputReceiver = inputReceiver;

            world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(
                map => _disposable.Disposable = map.ThrowAnimationEntities.SubscribeIncludingCurrentItems(Add, Remove),
                map => map.ThrowAnimationEntities.ForEach(entity => Remove(entity))
            );
        }

        protected override EntityView ViewPrefab(ThrowAnimationEntity _)
        {
            return ScriptableObjectLoader.LoadPrefab("Entity").GetComponent<EntityView>();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        ~SynchronizedThrowAnimationEntityView()
        {
            Dispose();
        }

        protected override void InitializeView(ThrowAnimationEntity eventEntity, EntityView entityView)
        {
            var spriteView = entityView.GetComponent<SpriteView>();
            spriteView.GetComponent<SpriteRenderer>().sprite = eventEntity.Icon;
        }

        protected override void CleanupView(ThrowAnimationEntity item, EntityView view)
        {
        }
    }
}