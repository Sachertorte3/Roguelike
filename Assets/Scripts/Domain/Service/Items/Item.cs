#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Service.Effect;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public class Item : BaseItem, ISerializable<ItemMemento>
    {
        private readonly ItemCategory _category;
        private readonly Option<ISkill> _skillOnUse;
        private readonly Option<ISkill> _skillOnThrow;
        private readonly bool _useOnDeath;
        private readonly bool _cannotUseIfCursed;
        private readonly bool _cannotDropIfCursed;
        private readonly bool _identifyIfGot;
        private readonly bool _identifyIfUsed;
        private readonly bool _autoDestroyWhenDisabled;

        public override string RevealedName => BaseName;
        public override ItemCategory Category => _category;
        public override Option<ISkill> SkillOnUse => _skillOnUse;
        public override Option<ISkill> SkillOnThrow => _skillOnThrow;
        protected override bool HasSameEffect => _hasSameEffect;
        protected override bool HasSameSkill => _hasSameSkill;
        public override bool UseOnDeath => _useOnDeath;
        public override Option<IStorage> ItemStorage => Option<IStorage>.None;
        public override bool CannotUseIfCursed => _cannotUseIfCursed;
        public override bool CannotDropIfCursed => _cannotDropIfCursed;
        public override bool IdentifyIfGot => _identifyIfGot;
        public override bool IdentifyIfUsed => _identifyIfUsed;
        public override bool AutoDestroyWhenDisabled => _autoDestroyWhenDisabled;
        public readonly IReadOnlyList<DirectWeaponFeature> FeaturesToMergeWeapon;

        public Item(ItemData data) : this(Build(data))
        {
        }

        public Item(ItemMemento data) : base(
            data.BaseItem)
        {
            _category = data.Category;
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
                            itemTargetSkill => throw new Exception("SkillOnUse is not SpawnEffectSkill"),
                            inventoryTargetSkill => throw new Exception("SkillOnUse is not SpawnEffectSkill")
                        ).Deserialize();
                    }
                    else if (data.HasSameSkill)
                    {
                        return _skillOnUse.Expect("SkillOnUse is null").Serialize().Match(
                            spawnEffectSkillOnUse => spawnEffectSkillOnUse.CopyWith(
                                probabilityOfSuccess: spawnEffectSkillMemento.ProbabilityOfSuccess
                            ),
                            itemTargetSkill => throw new Exception("SkillOnUse is not SpawnEffectSkill"),
                            inventoryTargetSkill => throw new Exception("SkillOnUse is not SpawnEffectSkill")
                        ).Deserialize();
                    }
                    else
                    {
                        return new SpawnEffectSkill(spawnEffectSkillMemento);
                    }
                },
                itemTargetSkillMemento => throw new Exception("SkillOnThrow is not SpawnEffectSkill"),
                inventoryTargetSkillMemento => throw new Exception("SkillOnThrow is not SpawnEffectSkill")
            ));
            _useOnDeath = data.UseOnDeath;
            _cannotUseIfCursed = data.CannotUseIfCursed;
            _cannotDropIfCursed = data.CannotDropIfCursed;
            _identifyIfGot = data.IdentifyIfGot;
            _identifyIfUsed = data.IdentifyIfUsed;
            _autoDestroyWhenDisabled = data.AutoDestroyWhenDisabled;
            FeaturesToMergeWeapon = data.FeaturesToMergeWeapon;
        }

        private readonly bool _hasSameEffect;
        private readonly bool _hasSameSkill;

        public ItemMemento Serialize()
        {
            var json = JsonUtility.ToJson(new ItemMemento
            (
                baseItem: SerializeBase(),
                category: _category,
                skillOnUse: _skillOnUse.Map(skill => skill.Serialize()),
                skillOnThrow: _skillOnThrow.Map(skill => skill.Serialize()),
                hasSameEffect: _hasSameEffect,
                hasSameSkill: _hasSameSkill,
                useOnDeath: _useOnDeath,
                cannotUseIfCursed: CannotUseIfCursed,
                cannotDropIfCursed: CannotDropIfCursed,
                identifyIfGot: _identifyIfGot,
                identifyIfUsed: _identifyIfUsed,
                autoDestroyWhenDisabled: _autoDestroyWhenDisabled,
                featuresToMergeWeapon: FeaturesToMergeWeapon.ToList()
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
                ItemEffectType.InventoryTarget => new InventoryTargetSkill(InventoryTargetSkill.Build(data.InventoryEffect)).Serialize(),
                ItemEffectType.None => null,
                _ => throw new Exception("Invalid item effect type")
            };
            var skillOnThrow = data.SpawnEffectsOnThrow
                ? (ISkillMemento)SpawnEffectSkill.Build(data.SkillOnThrow)
                : null;

            var json = JsonUtility.ToJson(new ItemMemento
            (
                baseItem: BuildBase(
                    baseName: data.name,
                    icon: data.Icon,
                    isShiny: data.IsShiny,
                    additionalPrice: data.AdditionalPrice,
                    multiplyPrice: data.MultiplyPrice,
                    state: state,
                    maxUsages: data.UsageLimit,
                    isCursed: isCursed,
                    upgradeLimit: data.UpgradeLimit,
                    conditions: data.PassiveConditions
                ),
                category: data.Category,
                skillOnUse: skillOnUse.ToOption(),
                skillOnThrow: skillOnThrow.ToOption(),
                hasSameEffect: data.IsSameEffect,
                hasSameSkill: data.IsSameSkill,
                useOnDeath: data.UseOnDeath,
                cannotUseIfCursed: data.CannotUseIfCursed,
                cannotDropIfCursed: data.CannotDropIfCursed,
                identifyIfGot: data.IdentifyIfGot,
                identifyIfUsed: data.IdentifyIfUsed,
                autoDestroyWhenDisabled: data.AutoDestroyWhenDisabled,
                featuresToMergeWeapon: data.FeaturesToMergeWeapon
            ));
            return JsonUtility.FromJson<ItemMemento>(json); //MEMO: To break the sharing of references
        }

        protected override string FullInfoImpl() => "";
    }
}