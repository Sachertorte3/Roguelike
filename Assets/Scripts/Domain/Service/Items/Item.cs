#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Service.Effect;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public class Item : BaseItem, ISerializable<ItemMemento>
    {
        private readonly Option<ISkill> _skillOnUse;
        private readonly Option<ISkill> _skillOnThrow;
        public override Option<ISkill> SkillOnUse => _skillOnUse;
        public override Option<ISkill> SkillOnThrow => _skillOnThrow;
        public override string RevealedName => BaseName;

        public Item(ItemData data) : this(Build(data))
        {
        }

        public Item(ItemMemento data) : base(
            data.Id, data.Category, data.BaseName, data.CustomName, data.Icon,
            data.IsShiny, data.State, data.UpgradePaths, data.HasSameEffect, data.HasSameSkill,
            data.UseOnDeath, data.Storage, data.MaxUsages, data.RemainingUsages, data.IsCursed,
            data.CannotUseIfCursed, data.CannotDropIfCursed, data.IdentifyIfGot, data.IdentifyIfUsed,
            data.IsCurseIdentified, data.AutoDestroyWhenDisabled, data.UpgradeLimit, data.Conditions.ToList())
        {
            _skillOnUse = data.SkillOnUse.Map(skill => skill.Deserialize());
            _skillOnThrow = data.SkillOnThrow.Map(skill => skill.Match(
                spawnEffectSkillMemento =>
                {
                    if (data.HasSameEffect)
                    {
                        return _skillOnUse.Expect("SkillOnUse is null").Serialize().Match(
                            spawnEffectSkillOnUse => spawnEffectSkillOnUse.CopyWith(
                                spawnEffectSkillMemento.Position,
                                spawnEffectSkillMemento.Area,
                                probabilityOfSuccess: spawnEffectSkillMemento.ProbabilityOfSuccess,
                                log: spawnEffectSkillMemento.Log
                            ),
                            itemTargetSkill => throw new Exception("SkillOnUse is not SpawnEffectSkill")
                        ).Deserialize();
                    }
                    else if (data.HasSameSkill)
                    {
                        return _skillOnUse.Expect("SkillOnUse is null").Serialize().Match(
                            spawnEffectSkillOnUse => spawnEffectSkillOnUse.CopyWith(
                                probabilityOfSuccess: spawnEffectSkillMemento.ProbabilityOfSuccess
                            ),
                            itemTargetSkill => throw new Exception("SkillOnUse is not SpawnEffectSkill")
                        ).Deserialize();
                    }
                    else
                    {
                        return new SpawnEffectSkill(spawnEffectSkillMemento);
                    }
                },
                itemTargetSkillMemento => new ItemTargetSkill(itemTargetSkillMemento)
            )); 
        }

        private readonly bool _hasSameEffect;
        private readonly bool _hasSameSkill;

        public ItemMemento Serialize()
        {
            var json = JsonUtility.ToJson(new ItemMemento
            (
                Id,
                Category,
                BaseName,
                CustomName,
                Icon,
                IsShiny,
                state: State,
                upgradePaths: _upgradePaths.Select(path => path.ToString()).ToList(),
                skillOnUse: _skillOnUse.Map(skill => skill.Serialize()),
                skillOnThrow: _skillOnThrow.Map(skill => skill.Serialize()),
                hasSameEffect: _hasSameEffect,
                hasSameSkill: _hasSameSkill,
                useOnDeath: UseOnDeath,
                storage: _itemStorage.Map(storage => storage.Serialize()),
                maxUsages: MaxUsages,
                remainingUsages: _remainingUsages.CurrentValue,
                isCursed: IsCursed,
                cannotUseIfCursed: CannotUseIfCursed,
                cannotDropIfCursed: CannotDropIfCursed,
                identifyIfGot: IdentifyIfGot,
                identifyIfUsed: IdentifyIfUsed,
                isCurseIdentified: IsCurseIdentified,
                autoDestroyWhenDisabled: AutoDestroyWhenDisabled,
                upgradeLimit: UpgradeLimit,
                conditions: _conditions
            ));
            return JsonUtility.FromJson<ItemMemento>(json);
        }

        public static ItemMemento Build(ItemData data, bool isCursed = false, ItemState state = ItemState.None)
        {
            var skillOnUse = data.EffectType switch
            {
                ItemEffectType.SpawnEffect => data.SpawnEffectsOnUse
                    ? (ISkillMemento)SpawnEffectSkill.Build(data.SkillOnUse)
                    : null,
                ItemEffectType.ItemTarget => new ItemTargetSkill(ItemTargetSkill.Build(data.ItemEffect)).Serialize(),
                _ => null
            };
            var skillOnThrow = data.SpawnEffectsOnThrow
                ? (ISkillMemento)SpawnEffectSkill.Build(data.SkillOnThrow)
                : null;

            var json = JsonUtility.ToJson(new ItemMemento
            (
                id: Id<IItem>.Generate(),
                category: data.Category,
                baseName: data.name,
                customName: Option<string>.None,
                icon: data.Icon,
                isShiny: data.IsShiny,
                state: state,
                upgradePaths: new List<string>(),
                skillOnUse: skillOnUse.ToOption(),
                skillOnThrow: skillOnThrow.ToOption(),
                hasSameEffect: data.IsSameEffect,
                hasSameSkill: data.IsSameSkill,
                useOnDeath: data.UseOnDeath,
                storage: data.StorageCapacity > 0 ? Storage.Build(data.StorageCapacity, false).ToOption() : Option<StorageMemento>.None,
                maxUsages: data.UsageLimit,
                remainingUsages: data.UsageLimit,
                isCursed: isCursed,
                cannotUseIfCursed: data.CannotUseIfCursed,
                cannotDropIfCursed: data.CannotDropIfCursed,
                identifyIfGot: data.IdentifyIfGot,
                identifyIfUsed: data.IdentifyIfUsed,
                isCurseIdentified: false,
                autoDestroyWhenDisabled: data.AutoDestroyWhenDisabled,
                upgradeLimit: data.UpgradeLimit,
                conditions: data.PassiveConditions
            ));
            return JsonUtility.FromJson<ItemMemento>(json); //MEMO: To break the sharing of references
        }
    }
}