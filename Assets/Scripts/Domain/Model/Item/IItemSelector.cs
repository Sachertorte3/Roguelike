#nullable enable
using Cysharp.Threading.Tasks;

namespace Domain.Model.Item
{
    public interface IItemSelector
    {
        public UniTask<ItemFocus> SelectItem(string text, params ItemFocus[] disabledItems);
    }
}