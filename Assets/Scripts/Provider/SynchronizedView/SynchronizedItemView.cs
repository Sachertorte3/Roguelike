#nullable enable
using System;
using Domain.Model.Item;
using Game;
using R3;
using UnityEngine;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedItemView : SynchronizedEntityView<IItemEntity, ItemView>, IDisposable
    {
        private readonly SerialDisposable _disposable = new();
        protected override InputReceiver _inputReceiver { get; init; }
        protected override GameManager _gameManager { get; init; }
        protected override World _world { get; init; }

        protected override EntityView GetEntityView(ItemView view)
        {
            return view.GetComponent<EntityView>();
        }

        [Inject]
        public SynchronizedItemView(World world, InputReceiver inputReceiver, GameManager gameManager)
        {
            _inputReceiver = inputReceiver;
            _gameManager = gameManager;
            _world = world;

            world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(
                map => _disposable.Disposable = map.Items.SubscribeIncludingCurrentItems(Add, Remove),
                map => map.Items.ForEach(item => Remove(item))
            );
        }

        protected override ItemView ViewPrefab(IItemEntity _)
        {
            return ScriptableObjectLoader.LoadPrefab("Item").GetComponent<ItemView>();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        ~SynchronizedItemView()
        {
            Dispose();
        }

        protected override void InitializeView(IItemEntity item, ItemView entityView)
        {
            var spriteView = entityView.GetComponent<SpriteView>();
            spriteView.GetComponent<SpriteRenderer>().sprite = item.Icon;

            var itemView = entityView.GetComponent<ItemView>();
            itemView.SetShiny(item.Item.IsShiny);
        }

        protected override void CleanupView(IItemEntity item, ItemView view)
        {
        }
    }
}