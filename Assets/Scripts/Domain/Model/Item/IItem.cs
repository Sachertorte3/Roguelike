#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Model.Item
{
    public interface IItem : IEquatable<IItem>, IHasUpgrades
    {
        public Id<IItem> Id { get; }
        public string BaseName { get; }
        public string UnknownName(ItemPlaceholders itemPlaceholders);
        public string RevealedName { get; }
        public Option<string> CustomName { get; }
        public string DebugName { get; }
        public string GetName(IPlayer player, ItemPlaceholders itemPlaceholders);
        public Sprite Icon { get; }
        public bool IsShiny { get; }
        public ItemState State { get; }
        public bool UseOnDeath { get; }
        public int Price { get; }
        public bool HasActivatableSkillWhenUsed { get; }
        public bool HasActivatableSkillWhenThrown { get; }
        public bool CanActivateWhenUsed { get; }
        public bool CanActivateWhenThrown { get; }
        public Option<ISkill> SkillOnUse { get; }
        public Option<ISkill> SkillOnThrow { get; }
        public bool HasActivatableSkill { get; }
        public bool CanActivate { get; }
        public float EvaluateWhenUsed(IActor actor, Vector2Int position, Direction8 direction, IMap map);
        public float EvaluateWhenThrown(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map);
        public bool IsDisabled { get; }
        public int MaxUsages { get; }
        public ReadOnlyReactiveProperty<int> RemainingUses { get; }
        public bool IsCursed { get; }
        public bool CannotUseIfCursed { get; }
        public bool RequiresLiteracy { get; }
        public bool CannotDropIfCursed { get; }
        public bool IdentifyIfGot { get; }
        public bool IdentifyIfUsed { get; }
        public bool IsCurseIdentified { get; }
        public bool IsInfoIdentified(IPlayer player);
        public bool AutoDestroyWhenDisabled { get; }
        public IReadOnlyList<UpgradePath> UpgradePaths { get; }
        public int AppliedUpgrades { get; }
        public IReadOnlyList<IConditionData> PassiveConditions { get; }
        public Observable<Unit> OnItemUpdated { get; }
        public Observable<bool> OnCursedChanged { get; }
        public void SetState(ItemState state);
        public UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map);

        public UniTask<ISkillResult> UseWhenThrown(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map);

        public void Repair(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders);
        public void SetCursed(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool isCursed);
        public void SetCurseIdentified(bool isCurseIdentified);
        public void Rename(string name);
        public void RevertToDefaultName();
        public bool CanUpgrade(string filter = "");
        public void RandomUpgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, string filter = "");
        public void Upgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, UpgradePath path);
        public void Downgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders);
        public string Info(IPlayer player, ItemPlaceholders itemPlaceholders);
        public string DebugInfo();
    }
}