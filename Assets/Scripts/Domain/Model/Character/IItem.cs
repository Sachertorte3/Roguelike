#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Condition;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Model.Item
{
    public interface IItem : ISerializable<ItemMemento>, IEquatable<IItem>, IHasUpgrades
    {
        public Id<IItem> Id { get; }
        public string BaseName { get; }
        public string UnknownName(ItemPlaceholders itemPlaceholders);
        public string RevealedName { get; }
        public string DebugName { get; }
        public string GetName(IHasInventory player, ItemPlaceholders itemPlaceholders);
        public Sprite Icon { get; }
        public bool IsShiny { get; }
        public ItemState State { get; }
        public bool UseOnDeath { get; }
        public int Price { get; }
        public bool HasActivatableSkillWhenUsed { get; }
        public bool HasActivatableSkillWhenThrown { get; }
        public bool CanActivateWhenUsedA { get; }
        public bool CanActivateWhenThrownA { get; }
        public Option<ISkill> SkillOnUse { get; }
        public Option<ISkill> SkillOnThrow { get; }
        public bool HasActivatableSkill { get; }
        public bool CanActivate { get; }
        public float EvaluateWhenUsed(IActor actor, Vector2Int position, Direction8 direction, IMap map);
        public float EvaluateWhenThrown(IActor actor, Vector2Int position, Direction8 direction, IMap map);
        public bool IsDisabled { get; }
        public int MaxUsages { get; }
        public ReadOnlyReactiveProperty<int> RemainingUses { get; }
        public bool IsCursed { get; }
        public bool CannotDropIfCursed { get; }
        public bool IdentifyIfGot { get; }
        public bool IdentifyIfUsed { get; }
        public bool IsCurseIdentified { get; }
        public bool AutoDestroyWhenDisabled { get; }
        public int AppliedUpgrades { get; }
        public IReadOnlyList<IConditionData> PassiveConditions { get; }
        public Observable<Unit> OnItemUpdated { get; }
        public Observable<bool> OnCursedChanged { get; }
        public void SetState(ItemState state);
        public UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map);

        public UniTask<ISkillResult> UseWhenThrown(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map);

        public void Repair(IHasInventory player, ItemPlaceholders itemPlaceholders);
        public void SetCursed(IHasInventory player, ItemPlaceholders itemPlaceholders, bool isCursed);
        public bool CanAnyUpgrade(string filter = "");
        public void RandomUpgrade(IHasInventory player, ItemPlaceholders itemPlaceholders, string filter = "");
        public void Upgrade(IHasInventory player, ItemPlaceholders itemPlaceholders, UpgradePath path);
        public void Downgrade(IHasInventory player, ItemPlaceholders itemPlaceholders);
        public string Info(IHasInventory player, ItemPlaceholders itemPlaceholders);
        public string DebugInfo();
    }
}