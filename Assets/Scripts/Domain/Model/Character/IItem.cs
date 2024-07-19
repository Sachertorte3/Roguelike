#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Effect;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Model.Item
{
    public interface IItem : ISerializable<ItemMemento>, IHasInfo
    {
        public Id<IItem> Id { get; }
        public string Name { get; }
        public Sprite Icon { get; }
        public ItemState State { get; }
        public bool EffectsOnUse => SkillOnUse != null;
        public bool EffectsOnThrow => SkillOnThrow != null;
        public bool Usable => EffectsOnUse || EffectsOnUse;
        public int Price { get; }
        public ISkill? SkillOnThrow { get; }
        public ISkill? SkillOnUse { get; }
        public bool IsDisabled { get; }
        public ReadOnlyReactiveProperty<int> RemainingUses { get; }
        public IReadOnlyList<IConditionData> PassiveConditions { get; }
        public Observable<Unit> OnItemUpdated { get; }
        public void SetState(ItemState state);
        public UniTask Use(IActor actor, Vector2Int position, Direction8 direction, IMap world, bool isThrown);
        public void Repair();
        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap world);
    }
}