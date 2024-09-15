#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Model.Item
{
    public interface IItem : ISerializable<ItemMemento>, IHasInfo, IHasUpgrades
    {
        public Id<IItem> Id { get; }
        public string Name { get; }
        public Sprite Icon { get; }
        public ItemState State { get; }
        public bool UseOnDeath { get; }
        public int Price { get; }
        public bool CanActivateWhenUsed { get; }
        public bool CanActivateWhenThrown { get; }
        public Option<ISkill> SkillOnUse { get; }
        public Option<ISkill> SkillOnThrow { get; }
        public bool Usable => CanActivateWhenUsed || CanActivateWhenThrown;
        public float EvaluateWhenUsed(IActor actor, Vector2Int position, Direction8 direction, IMap world);
        public float EvaluateWhenThrown(IActor actor, Vector2Int position, Direction8 direction, IMap world);
        public bool IsDisabled { get; }
        public int MaxUsages { get; }
        public ReadOnlyReactiveProperty<int> RemainingUses { get; }
        public IReadOnlyList<IConditionData> PassiveConditions { get; }
        public Observable<Unit> OnItemUpdated { get; }
        public void SetState(ItemState state);
        public UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap world);
        public UniTask<ISkillResult> UseWhenThrown(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap world);
        public void Repair();
        public bool CanUpgrade(string filter = "");
        public void Upgrade(string filter = "");
    }
}