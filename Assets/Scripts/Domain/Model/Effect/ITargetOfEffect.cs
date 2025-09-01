using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface ITargetOfEffect : IHasName, IHasStatus, IHasInventory, IEntity
    {
        public int DropExp { get; }
        public IVisionRange VisionRange { get; }
        public void AddCondition(Id<IEntity> actor, ConditionTemplate condition);
        public void ClearCondition();
        public void ClearKnownItems(IMap map);
        public void ClearAffiliation(IMap map);
        public void ListenToAlert(Location location);
        public void DropItem(ItemFocus index, IMap map, bool isForced = false);
    }
}