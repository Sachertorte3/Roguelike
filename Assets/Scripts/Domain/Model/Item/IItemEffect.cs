#nullable enable
using System.Collections.Generic;

namespace Domain.Model.Item
{
    public interface IItemEffect : IHasInfo
    {
        public IEnumerable<int> GetDisabledItemIndexes(IInventory inventory);
        public void Apply(IItem item);
        public float EvaluatePrice();
    }
}