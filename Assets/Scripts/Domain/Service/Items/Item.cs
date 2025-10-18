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
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public class Item : BaseItem, ISerializable<ItemMemento>
    {
        private readonly ItemCategory _category;
        private readonly Option<ISkill> _skillOnUse;
        private readonly Option<ISkill> _skillOnThrow;
        private readonly bool _useOnDeath;

        public override string RevealedName => BaseName;
        public override ItemCategory Category => _category;
        public override Option<ISkill> SkillOnUse => _skillOnUse;
        public override Option<ISkill> SkillOnThrow => _skillOnThrow;
        protected override bool HasSameEffect => _hasSameEffect;
        protected override bool HasSameSkill => _hasSameSkill;
        public override bool UseOnDeath => _useOnDeath;
        public override Option<IStorage> ItemStorage => Option<IStorage>.None;
        public bool CanMergeUses => Category == ItemCategory.Books || Category == ItemCategory.Wands;
        public override bool CannotUseIfCursed => Category != ItemCategory.Weapons;
        public override bool RequiresLiteracy => Category == ItemCategory.Books || Category == ItemCategory.Scrolls;
        public override bool CannotDropIfCursed => Category == ItemCategory.Weapons;
        public override bool IdentifyIfGot => Category == ItemCategory.Weapons || Category == ItemCategory.Others;
        public override bool IdentifyIfUsed => Category != ItemCategory.Wands;
        public override bool AutoDestroyWhenDisabled => Category == ItemCategory.Potions || Category == ItemCategory.Scrolls || Category == ItemCategory.Others;
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
                featuresToMergeWeapon: FeaturesToMergeWeapon.ToList()
            ));
            return JsonUtility.FromJson<ItemMemento>(json);
        }

        public ItemMemento SerializeIgnoreUpgrades()
        {
            foreach (var upgradePath in UpgradePaths)
            {
                this.ApplyDowngrade(upgradePath);
            }
            var memento = Serialize();
            foreach (var upgradePath in UpgradePaths)
            {
                this.ApplyUpgrade(upgradePath);
            }
            return memento;
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
                    usageLossChance: 1,
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
                featuresToMergeWeapon: data.FeaturesToMergeWeapon
            ));
            return JsonUtility.FromJson<ItemMemento>(json); //MEMO: To break the sharing of references
        }

        protected override string FullInfoImpl() => "";

        public Item Merge(Item mergedItem)
        {
            if (BaseName != mergedItem.BaseName)
            {
                throw new Exception("Cannot merge different items");
            }
            else if (!CanMergeUses)
            {
                throw new Exception("Cannot merge uses");
            }
            var memento = Serialize();
            var mergedItemIgnoreUpgrade = new Item(mergedItem.SerializeIgnoreUpgrades());
            var item = new Item(memento.CopyWith(
                baseItem: memento.BaseItem.CopyWith(
                    maxUsages: MaxUsages + mergedItemIgnoreUpgrade.MaxUsages,
                    remainingUsages: RemainingUses.CurrentValue + mergedItem.RemainingUses.CurrentValue
                //MEMO: It's not wrong to use values ​​that include upgrades here.
                )
            ));
            foreach (var upgradePath in mergedItem.UpgradePaths.Shuffled())
            {
                if (item.CanUpgrade(upgradePath.ToString()))
                {
                    item.UpgradeNoLog(upgradePath);
                }
            }
            return item;
        }
    }
}