#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Model.Character
{
    public interface IHasInventory
    {
        public IInventory Inventory { get; }
        public void KnowItem(IItem item, bool log);
        public bool IsKnownItem(IItem item);
        public UniTask<ItemFocus> SelectItem(string text, params ItemFocus[] disabledItems);
        public UniTask<ItemFocus> SelectItemWithCanSelect(string text, IPlayer player, IMap map, Func<IItem, bool> canSelect);
    }
}