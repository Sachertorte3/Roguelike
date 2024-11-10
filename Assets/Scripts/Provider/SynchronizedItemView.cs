#nullable enable
using System;
using System.Linq;
using Domain.Model;
using Domain.Model.Item;
using Domain.Model.Setting;
using Game;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedItemView : SynchronizedEntityView<IItemEntity, ItemView>, IDisposable
    {
        private readonly SerialDisposable _disposable = new();
        protected override InputReceiver _inputReceiver { get; init; }

        protected override EntityView GetEntityView(ItemView view)
        {
            return view.GetComponent<EntityView>();
        }

        [Inject]
        public SynchronizedItemView(World world, InputReceiver inputReceiver)
        {
            _inputReceiver = inputReceiver;

            world.ActiveMap.SubscribeToAllItemsIgnoreNull(
                map => _disposable.Disposable = map.Items.SubscribeToAllItems(Add, Remove),
                map => map.Items.ForEach(item => Remove(item))
            );
        }

        protected override ItemView ViewPrefab(IItemEntity _)
        {
            return Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Item.prefab").WaitForCompletion()
                .GetComponent<ItemView>();
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