#nullable enable
using ObservableCollections;
using R3;
using Scripts.Model.Items;
using System.Collections.ObjectModel;
using System.Linq;

namespace Assets.Scripts.Model.Items
{
    internal class Inventory : IInventory
    {
        private const int MaxItems = 10;
        public ReadOnlyCollection<Item?> Items => new(_items);
        public Observable<CollectionReplaceEvent<Item?>> OnItemChanged => _items.ObserveReplace();
        private ObservableList<Item?> _items = new(Enumerable.Repeat<Item?>(null, MaxItems));
        public bool HasEmptySpace() => _items.IndexOf(null) >= 0;
        public bool TryAdd(Item item)
        {
            int index = _items.IndexOf(null);
            if (index >= 0)
            {
                Replace(item, index);
                return true;
            }
            else
            {
                return false;
            }
        }
        public Item? Replace(Item? item, int index)
        {
            Item? removed = _items[index];
            _items[index] = item;
            return removed;
        }
    }
}
