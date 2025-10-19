#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Service.Effect;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public class DirectWeapon : BaseItem, ISerializable<DirectWeaponMemento>
    {
        public override string RevealedName => _prefix.MapOr("", prefix => prefix.Name) + BaseName;
        public override ItemCategory Category => ItemCategory.Weapons;
        private bool _hasSameEffect;
        protected override bool HasSameEffect => _hasSameEffect;
        protected override bool HasSameSkill => false;
        public override bool UseOnDeath => false;
        public override bool CannotUseIfCursed => false;
        public override bool RequiresLiteracy => false;
        public override bool CannotDropIfCursed => true;
        public override bool IdentifyIfGot => true;
        public override bool IdentifyIfUsed => true;
        public override bool AutoDestroyWhenDisabled => false;
        private readonly Option<WeaponPrefix> _prefix;
        private readonly List<ElementPower> _elementPowers;
        private readonly List<DirectWeaponFeature> _features;
        public IReadOnlyList<DirectWeaponFeature> Features => _features;
        public readonly int FeatureLimit;
        private readonly SpawnEffectSkill _skillOnUse;
        private readonly SpawnEffectSkill _skillOnThrow;
        public override Option<ISkill> SkillOnUse => ((ISkill)_skillOnUse).ToOption();
        public override Option<ISkill> SkillOnThrow => ((ISkill)_skillOnThrow).ToOption();
        public DirectWeapon(DirectWeaponData data) : this(Build(data))
        {
        }

        public DirectWeapon(DirectWeaponMemento data) : base(data.BaseItem)
        {
            _prefix = data.Prefix;
            _elementPowers = data.ElementPowers;
            _features = data.Features;
            FeatureLimit = data.FeatureLimit;
            _skillOnUse = new SpawnEffectSkill(data.SkillOnUse);
            _skillOnThrow = new SpawnEffectSkill(data.SkillOnThrow);
        }

        public DirectWeaponMemento Serialize()
        {
            var json = JsonUtility.ToJson(new DirectWeaponMemento
            (
                baseItem: SerializeBase(),
                prefix: _prefix,
                elementPowers: _elementPowers,
                features: _features,
                featureLimit: FeatureLimit,
                skillOnUse: _skillOnUse.Serialize(),
                skillOnThrow: _skillOnThrow.Serialize(),
                hasSameEffect: _hasSameEffect
            ));
            return JsonUtility.FromJson<DirectWeaponMemento>(json);
        }

        public DirectWeaponMemento SerializeIgnoreUpgrades()
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

        public static (SpawnEffectSkill skillOnUse, SpawnEffectSkill skillOnThrow, bool hasSameEffect) BuildSkills(List<ElementPower> elementPowers, List<DirectWeaponFeature> features, WeaponPrefix? prefix = null, bool skipMultiplyPower = false)
        {
            var range = features.Contains(DirectWeaponFeature.TwoRangeAttack) ? 2 : 1;
            var area = (IArea)new LineArea(range, false, false);
            if (features.Contains(DirectWeaponFeature.FanAttack))
            {
                area = new FanArea(range, false, false);
            }
            else if (features.Contains(DirectWeaponFeature.SpinAttack))
            {
                area = new CircleArea(range, false, false);
            }

            var effectsOnUse = new List<IEffect>();
            var effectsOnThrow = new List<IEffect>();
            if (prefix != null && !skipMultiplyPower)
            {
                elementPowers = elementPowers.Select(power => power.MultiplyPower(prefix.PowerMagnification)).ToList();
            }
            var criticalRate = features.Count(f => f == DirectWeaponFeature.Critical) * 0.25f;
            var throwEnhance = features.Contains(DirectWeaponFeature.EnhanceThrow) ? 1.5f : 1f;
            var hasSameEffect = throwEnhance == 1f;
            if (features.Contains(DirectWeaponFeature.Absorbing))
            {
                var absorbRate = features.Count(f => f == DirectWeaponFeature.Absorbing) * 0.25f;
                effectsOnUse.Add(new AbsorbsEffect(
                    elementPowers,
                    absorbRate,
                    criticalRate
                ));
                effectsOnThrow.Add(new AbsorbsEffect(
                    elementPowers.Select(power => power.MultiplyPower(throwEnhance)).ToList(),
                    absorbRate,
                    criticalRate
                ));
            }
            else
            {
                effectsOnUse.Add(new AttackEffect(
                    elementPowers,
                    criticalRate
                ));
                effectsOnThrow.Add(new AttackEffect(
                    elementPowers.Select(power => power.MultiplyPower(throwEnhance)).ToList(),
                    criticalRate
                ));
            }
            if (features.Contains(DirectWeaponFeature.Knockback))
            {
                effectsOnUse.Add(new BlowAwayEffect(1));
                effectsOnThrow.Add(new BlowAwayEffect(1));
            }
            if (features.Contains(DirectWeaponFeature.Dig))
            {
                effectsOnUse.Add(new DigEffect());
                effectsOnThrow.Add(new DigEffect());
            }
            if (features.Contains(DirectWeaponFeature.BreakTrap))
            {
                effectsOnUse.Add(new BreakEffect(false, false, false, true, false, false));
                effectsOnThrow.Add(new BreakEffect(false, false, false, true, false, false));
            }
            var abnormalConditionMultiplier = features.Count(f => f == DirectWeaponFeature.EnhanceAbnormalCondition) + 1;
            if (features.Contains(DirectWeaponFeature.Paralysis))
            {
                var probability = 0.05f * abnormalConditionMultiplier;
                var paralysis = new AdditionalConditionData(
                    ObjectLoader.Load<ConditionTemplate>("麻痺"), probability);
                effectsOnUse.Add(new AddConditionEffect(paralysis));
                effectsOnThrow.Add(new AddConditionEffect(paralysis));
            }
            if (features.Contains(DirectWeaponFeature.Blind))
            {
                var probability = 0.1f * abnormalConditionMultiplier;
                var blind = new AdditionalConditionData(
                    ObjectLoader.Load<ConditionTemplate>("盲目"), probability);
                effectsOnUse.Add(new AddConditionEffect(blind));
                effectsOnThrow.Add(new AddConditionEffect(blind));
            }
            if (features.Contains(DirectWeaponFeature.Confusion))
            {
                var probability = 0.1f * abnormalConditionMultiplier;
                var confusion = new AdditionalConditionData(
                    ObjectLoader.Load<ConditionTemplate>("混乱"), probability);
                effectsOnUse.Add(new AddConditionEffect(confusion));
                effectsOnThrow.Add(new AddConditionEffect(confusion));
            }
            if (features.Contains(DirectWeaponFeature.Sleep))
            {
                var probability = 0.05f * abnormalConditionMultiplier;
                var sleep = new AdditionalConditionData(
                    ObjectLoader.Load<ConditionTemplate>("睡眠"), probability);
                effectsOnUse.Add(new AddConditionEffect(sleep));
                effectsOnThrow.Add(new AddConditionEffect(sleep));
            }
            if (features.Contains(DirectWeaponFeature.Poison))
            {
                var probability = 0.2f * abnormalConditionMultiplier;
                var poison = new AdditionalConditionData(
                    ObjectLoader.Load<ConditionTemplate>("毒"), probability);
                effectsOnUse.Add(new AddConditionEffect(poison));
                effectsOnThrow.Add(new AddConditionEffect(poison));
            }
            if (features.Contains(DirectWeaponFeature.Slowness))
            {
                var probability = 0.1f * abnormalConditionMultiplier;
                var slowness = new AdditionalConditionData(
                    ObjectLoader.Load<ConditionTemplate>("鈍足"), probability);
                effectsOnUse.Add(new AddConditionEffect(slowness));
                effectsOnThrow.Add(new AddConditionEffect(slowness));
            }
            if (features.Contains(DirectWeaponFeature.Restraint))
            {
                var probability = 0.1f * abnormalConditionMultiplier;
                var restraint = new AdditionalConditionData(
                    ObjectLoader.Load<ConditionTemplate>("拘束"), probability);
                effectsOnUse.Add(new AddConditionEffect(restraint));
                effectsOnThrow.Add(new AddConditionEffect(restraint));
            }

            var repeat = features.Contains(DirectWeaponFeature.DoubleAttack) ? 2 : 1;

            var skillOnUseProbabilityOfSuccess = features.Contains(DirectWeaponFeature.GuaranteedHit) ? 1f : features.Contains(DirectWeaponFeature.Critical) ? 0.75f : CommonSenseParameters.SkillOnUseProbabilityOfSuccess;

            var skillOnThrowProbabilityOfSuccess = features.Contains(DirectWeaponFeature.GuaranteedHit) ? 1f : features.Contains(DirectWeaponFeature.Critical) ? 0.7f : CommonSenseParameters.SkillOnThrowProbabilityOfSuccess;

            var skillOnUse = new SpawnEffectSkill(SpawnEffectSkill.Build(
                new SkillData(
                    new AtFeet(),
                    area,
                    effectsOnUse,
                    repeat,
                    skillOnUseProbabilityOfSuccess,
                    "")
            ));
            var skillOnThrow = new SpawnEffectSkill(SpawnEffectSkill.Build(
                new SkillData(
                    new AtFeet(),
                    new SelfArea(),
                    effectsOnThrow,
                    1,
                    skillOnThrowProbabilityOfSuccess,
                    "")
            ));
            return (skillOnUse, skillOnThrow, hasSameEffect);
        }

        public static DirectWeaponMemento Build(DirectWeaponData data, WeaponPrefix? prefix = null, bool isCursed = false, ItemState state = ItemState.None, EnemyData? mimic = null)
        {
            var (skillOnUse, skillOnThrow, hasSameEffect) = BuildSkills(data.ElementPowers, data.Features, prefix);
            var multiplyPrice = data.Features.Contains(DirectWeaponFeature.Artistic) ? 2f : 1f;
            var usageLossChance = 1 - data.Features.Count(f => f == DirectWeaponFeature.EnhanceDurability) * 0.2f;
            var featureLimit = data.FeatureLimit + prefix?.FeatureLimitAdditional ?? 0;
            var maxUsages = Mathf.RoundToInt(data.UsageLimit * (prefix?.UsageLimitMagnification ?? 1f));

            var json = JsonUtility.ToJson(new DirectWeaponMemento
            (
                baseItem: BuildBase(
                    baseName: data.name,
                    icon: data.Icon,
                    isShiny: data.IsShiny,
                    additionalPrice: 0,
                    multiplyPrice: multiplyPrice,
                    state: state,
                    maxUsages: maxUsages,
                    usageLossChance: usageLossChance,
                    isCursed: isCursed,
                    upgradeLimit: data.UpgradeLimit + prefix.ToOption().MapOr(0, prefix => prefix.AdditionalUpgradeLimit),
                    conditions: data.PassiveConditions,
                    mimic: mimic.ToOption()
                ),
                prefix: prefix.ToOption(),
                elementPowers: data.ElementPowers,
                features: data.Features,
                featureLimit: data.FeatureLimit,
                skillOnUse: skillOnUse.Serialize(),
                skillOnThrow: skillOnThrow.Serialize(),
                hasSameEffect: hasSameEffect
            ));
            var item = JsonUtility.FromJson<DirectWeaponMemento>(json); //MEMO: To break the sharing references
            return item;
        }

        private DirectWeapon Merge(IEnumerable<DirectWeaponFeature> featuresToMergeWeapon, IEnumerable<UpgradePath> upgradePaths)
        {
            //MEMO: There is also a way to reload the data and regenerate it from scratch.
            var memento = SerializeIgnoreUpgrades();
            var features = memento.Features.Merge(featuresToMergeWeapon, memento.FeatureLimit).ToList();

            var (skillOnUse, skillOnThrow, hasSameEffect) = BuildSkills(memento.ElementPowers, features, memento.Prefix.Value, true);
            var multiplyPrice = features.Contains(DirectWeaponFeature.Artistic) ? 2f : 1f;
            var usageLossChance = 1 - features.Count(f => f == DirectWeaponFeature.EnhanceDurability) * 0.2f;
            var item = new DirectWeapon(memento.CopyWith(
                baseItem: memento.BaseItem.CopyWith(
                    multiplyPrice: multiplyPrice,
                    usageLossChance: usageLossChance
                ),
                features: features,
                skillOnUse: skillOnUse.Serialize(),
                skillOnThrow: skillOnThrow.Serialize(),
                hasSameEffect: hasSameEffect
            ));
            foreach (var upgradePath in item.UpgradePaths)
            {
                item.ApplyUpgrade(upgradePath);
            }
            foreach (var upgradePath in upgradePaths.Shuffled())
            {
                if (item.CanUpgrade(upgradePath.ToString()))
                {
                    item.UpgradeNoLog(upgradePath);
                }
            }
            return item;
        }

        public DirectWeapon Merge(DirectWeapon mergedItem) => Merge(mergedItem._features, mergedItem.UpgradePaths);
        public DirectWeapon Merge(Item mergedItem) => Merge(mergedItem.FeaturesToMergeWeapon, mergedItem.UpgradePaths);
        public DirectWeapon Merge(IItem mergedItem) => mergedItem switch
        {
            DirectWeapon weapon => Merge(weapon),
            Item item => Merge(item),
            _ => throw new Exception("Invalid item")
        };

        protected override string FullInfoImpl()
        {
            var info = "";

            info += $"能力 ({_features.Count}/{FeatureLimit})\n";

            foreach (var feature in _features)
            {
                info += $"{feature.GetName()}\n";
            }

            info += "\n";

            return info;
        }
    }
}