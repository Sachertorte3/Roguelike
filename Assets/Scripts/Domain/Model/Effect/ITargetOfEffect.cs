using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Entity;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface ITargetOfEffect : IHasName, IHasStatus, IHasInventory, IEntity
    {
        public IVisionRange VisionRange { get; }
        public void AddCondition(Id<IEntity> actor, IConditionData condition, RemovalConditionData removalCondition);
        public void ClearCondition();
        public void ClearKnownItems(IMap map);
        public void ClearAffiliation(IMap map);
        public void ListenToAlert(Vector2Int position);
        public void DropItem(int itemIndex, IMap map, bool isForced);
    }
}