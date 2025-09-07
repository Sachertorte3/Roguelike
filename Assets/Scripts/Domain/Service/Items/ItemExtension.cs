#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;

namespace Domain.Service.Items
{
    public static class ItemExtension
    {
        public static TResult Match<TResult>(this IItem item, Func<Item, TResult> itemFunc,
            Func<DirectWeapon, TResult> directWeaponFunc, Func<StorageItem, TResult> storageItemFunc)
        {
            return item switch
            {
                Item uniqueItem => itemFunc(uniqueItem),
                DirectWeapon directWeapon => directWeaponFunc(directWeapon),
                StorageItem storageItem => storageItemFunc(storageItem),
                _ => throw new ArgumentException("Invalid item type")
            };
        }

        public static async UniTask<TResult> Match<TResult>(this IItem item,
            Func<Item, UniTask<TResult>> itemFunc,
            Func<DirectWeapon, UniTask<TResult>> directWeaponFunc, Func<StorageItem, UniTask<TResult>> storageItemFunc)
        {
            return item switch
            {
                Item uniqueItem => await itemFunc(uniqueItem),
                DirectWeapon directWeapon => await directWeaponFunc(directWeapon),
                StorageItem storageItem => await storageItemFunc(storageItem),
                _ => throw new ArgumentException("Invalid item type")
            };
        }

        public static TResult Match<TResult>(this IItemMemento memento,
            Func<ItemMemento, TResult> itemFunc,
            Func<DirectWeaponMemento, TResult> directWeaponFunc, Func<StorageItemMemento, TResult> storageItemFunc)
        {
            return memento switch
            {
                ItemMemento itemMemento => itemFunc(itemMemento),
                DirectWeaponMemento directWeaponMemento => directWeaponFunc(directWeaponMemento),
                StorageItemMemento storageItemMemento => storageItemFunc(storageItemMemento),
                _ => throw new ArgumentException("Invalid item type")
            };
        }

        public static TResult Match<TResult>(this IItemData data,
            Func<ItemData, TResult> itemFunc,
            Func<DirectWeaponData, TResult> directWeaponFunc, Func<StorageItemData, TResult> storageItemFunc)
        {
            return data switch
            {
                ItemData itemData => itemFunc(itemData),
                DirectWeaponData directWeaponData => directWeaponFunc(directWeaponData),
                StorageItemData storageItemData => storageItemFunc(storageItemData),
                _ => throw new ArgumentException("Invalid item type")
            };
        }

        public static IItemMemento Serialize(this IItem item)
        {
            return item.Match(
                item => item.Serialize(),
                directWeapon => directWeapon.Serialize(),
                storageItem => (IItemMemento)storageItem.Serialize()
            );
        }

        public static IItem Deserialize(this IItemMemento memento)
        {
            return memento.Match<IItem>(
                itemMemento => new Item(itemMemento),
                directWeaponMemento => new DirectWeapon(directWeaponMemento),
                storageItemMemento => new StorageItem(storageItemMemento)
            );
        }

        public static IItemMemento Build(this IItemData data, bool isCursed = false, ItemState state = ItemState.None)
        {
            return data.Match(
                itemData => Item.Build(itemData, isCursed: isCursed, state: state),
                directWeaponData => DirectWeapon.Build(directWeaponData, isCursed: isCursed, state: state),
                storageItemData => (IItemMemento)StorageItem.Build(storageItemData, isCursed: isCursed, state: state)
            );
        }
    }
}