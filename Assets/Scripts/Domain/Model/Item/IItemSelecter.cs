#nullable enable
using Cysharp.Threading.Tasks;

namespace Domain.Model.Item
{
    public interface IItemSelecter
    {
        public UniTask<IItem?> SelectItem(IInventory inventory, params int[] disabledItemIds);
    }
}