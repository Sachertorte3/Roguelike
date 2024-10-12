#nullable enable
using System.Collections.Generic;
using Domain.Model.Dungeon;

namespace Domain.Model.Item
{
    public interface IItemEffect : IHasInfo
    {
        public IEnumerable<int> GetDisabledItemIndexes(IHasInventory actor);
        public void Apply(IHasInventory actor, IItem item, ItemDatabase itemDatabase);
        public float EvaluatePrice();
    }
}