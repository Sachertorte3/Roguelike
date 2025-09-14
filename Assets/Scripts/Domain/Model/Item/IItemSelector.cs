#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Model.Item
{
    public interface IItemSelector
    {
        public UniTask<ItemFocus> SelectItem(string text, IInventory inventory, IMap map, params ItemFocus[] disabledItems);
    }
}