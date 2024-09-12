#nullable enable
using Cysharp.Threading.Tasks;

namespace Domain.Model.Item
{
    public interface IItemSelector
    {
        public UniTask<IItem?> SelectItem(IInventory inventory, params int[] disabledItemIds);
    }
}