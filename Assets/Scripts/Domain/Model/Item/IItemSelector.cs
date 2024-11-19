#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Model.Item
{
    public interface IItemSelector
    {
        public UniTask<IItem?> SelectItem(IInventory inventory, IMap map, params int[] disabledItemIds);
    }
}