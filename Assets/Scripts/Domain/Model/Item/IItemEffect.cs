#nullable enable
using System.Collections.Generic;

namespace Domain.Model.Item
{
    public interface IItemEffect : IHasInfo
    {
        public IEnumerable<int> GetDisabledItemIndexes(IHasInventory actor);
        public void Apply(IHasInventory actor, IItem item);
        public float EvaluatePrice();
    }
}