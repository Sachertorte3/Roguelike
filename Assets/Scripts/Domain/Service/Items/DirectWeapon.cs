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
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Service.Effect;
using Domain.Service.Logs;
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
        public override bool RequiresLiteracy => false;
        public override bool IdentifyIfGot => true;
        public override bool IdentifyIfUsed => true;
        public override bool AutoDestroyWhenDisabled => false;
        private readonly Option<WeaponPrefix> _prefix;
        private readonly int _defaultPower;
        private readonly List<ItemFeature> _features;
        public IReadOnlyList<ItemFeature> Features => _features;
        public readonly int FeatureLimit;
        private SpawnEffectSkill _skillOnUse;
        private SpawnEffectSkill _skillOnThrow;
        public override Option<ISkill> SkillOnUse => ((ISkill)_skillOnUse).ToOption();
        public override Option<ISkill> SkillOnThrow => ((ISkill)_skillOnThrow).ToOption();
        public DirectWeapon(DirectWeaponData data) : this(Build(data))
        {
        }

        public DirectWeapon(DirectWeaponMemento data) : base(data.BaseItem)
        {
            _prefix = data.Prefix;
            _defaultPower = data.DefaultPower;
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
                defaultPower: _defaultPower,
                features: _features,
                featureLimit: FeatureLimit,
                skillOnUse: _skillOnUse.Serialize(),
                skillOnThrow: _skillOnThrow.Serialize(),
                hasSameEffect: _hasSameEffect
            ));
            return JsonUtility.FromJson<DirectWeaponMemento>(json);
        }

        public override bool CanUpgrade() => UpgradeCount < UpgradeLimit;
        public override bool CanDowngrade() => UpgradeCount > 0;
        public override void Upgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            GameLog.Add(itemHolder.IsVisible, $"{GetName(player, itemPlaceholders)}は強化された");
            UpgradeCount++;
            var (skillOnUse, skillOnThrow, hasSameEffect) = BuildSkills(_defaultPower, UpgradeCount, _features, _prefix.Value);
            _skillOnUse = new SpawnEffectSkill(skillOnUse);
            _skillOnThrow = new SpawnEffectSkill(skillOnThrow);
            _hasSameEffect = hasSameEffect;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public override void Downgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            GameLog.Add(itemHolder.IsVisible, $"{GetName(player, itemPlaceholders)}は強化が解除された");
            UpgradeCount--;
            var (skillOnUse, skillOnThrow, hasSameEffect) = BuildSkills(_defaultPower, UpgradeCount, _features, _prefix.Value);
            _skillOnUse = new SpawnEffectSkill(skillOnUse);
            _skillOnThrow = new SpawnEffectSkill(skillOnThrow);
            _hasSameEffect = hasSameEffect;
            _onItemUpdated.OnNext(Unit.Default);
        }
        public static (SpawnEffectSkillMemento skillOnUse, SpawnEffectSkillMemento skillOnThrow, bool hasSameEffect) BuildSkills(int power, int upgradeCount, List<ItemFeature> features, WeaponPrefix? prefix = null)
        {
            var range = features.Contains(ItemFeature.TwoRangeAttack) ? 2 : 1;
            var area = (IArea)new LineArea(range, false, false);
            if (features.Contains(ItemFeature.FanAttack))
            {
                area = new FanArea(range, false, false);
            }
            else if (features.Contains(ItemFeature.SpinAttack))
            {
                area = new CircleArea(range, false, false);
            }

            var effectsOnUse = new List<IEffect>();
            var effectsOnThrow = new List<IEffect>();
            var elementPowers = new List<ElementPower>();
            if (prefix != null)
            {
                power = Mathf.RoundToInt(power * prefix.PowerMagnification);
            }
            power += upgradeCount;
            var elementPower = Mathf.CeilToInt(power / 2f);

            var elementFeatureMapping = new Dictionary<ItemFeature, Element>
            {
                { ItemFeature.Fire, Element.Fire },
                { ItemFeature.Ice, Element.Ice },
                { ItemFeature.Thunder, Element.Thunder },
                { ItemFeature.Light, Element.Light },
                { ItemFeature.Dark, Element.Dark }
            };

            var elementFeature = elementFeatureMapping.Keys.FirstOrDefault(feature => features.Contains(feature));

            if (elementFeature != default)
            {
                var element = elementFeatureMapping[elementFeature];
                elementPowers.Add(new ElementPower(element, elementPower));
                elementPowers.Add(new ElementPower(Element.Physical, power - elementPower));
            }
            else
            {
                elementPowers.Add(new ElementPower(Element.Physical, power));
            }

            var criticalRate = features.Count(f => f == ItemFeature.Critical) * 0.25f;
            var throwEnhance = features.Contains(ItemFeature.EnhanceThrow) ? 1.5f : 1f;
            var hasSameEffect = throwEnhance == 1f;
            if (features.Contains(ItemFeature.Absorbing))
            {
                var absorbRate = features.Count(f => f == ItemFeature.Absorbing) * 0.25f;
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
            if (features.Contains(ItemFeature.Knockback))
            {
                effectsOnUse.Add(new BlowAwayEffect(1));
                effectsOnThrow.Add(new BlowAwayEffect(1));
            }
            if (features.Contains(ItemFeature.Dig))
            {
                effectsOnUse.Add(new DigEffect());
                effectsOnThrow.Add(new DigEffect());
            }
            if (features.Contains(ItemFeature.BreakTrap))
            {
                effectsOnUse.Add(new BreakEffect(false, false, false, true, false, false));
                effectsOnThrow.Add(new BreakEffect(false, false, false, true, false, false));
            }

            var abnormalConditionMultiplier = features.Count(f => f == ItemFeature.EnhanceAbnormalCondition) + 1;

            // 状態異常フィーチャーから対応するデータを取得
            var conditionFeatureMapping = new Dictionary<ItemFeature, (string templateName, float baseProbability)>
            {
                { ItemFeature.Paralysis, ("麻痺", 0.05f) },
                { ItemFeature.Blind, ("盲目", 0.1f) },
                { ItemFeature.Confusion, ("混乱", 0.1f) },
                { ItemFeature.Sleep, ("睡眠", 0.05f) },
                { ItemFeature.Poison, ("毒", 0.2f) },
                { ItemFeature.Slowness, ("鈍足", 0.1f) },
                { ItemFeature.Restraint, ("拘束", 0.1f) }
            };

            foreach (var (feature, (templateName, baseProbability)) in conditionFeatureMapping)
            {
                if (features.Contains(feature))
                {
                    var probability = baseProbability * abnormalConditionMultiplier;
                    var conditionData = new AdditionalConditionData(
                        ObjectLoader.Load<ConditionTemplate>(templateName), probability);
                    effectsOnUse.Add(new AddConditionEffect(conditionData));
                    effectsOnThrow.Add(new AddConditionEffect(conditionData));
                }
            }

            var repeat = features.Contains(ItemFeature.DoubleAttack) ? 2 : 1;

            var skillOnUseProbabilityOfSuccess = features.Contains(ItemFeature.GuaranteedHit) ? 1f : features.Contains(ItemFeature.Critical) ? 0.75f : CommonSenseParameters.SkillOnUseProbabilityOfSuccess;

            var skillOnThrowProbabilityOfSuccess = features.Contains(ItemFeature.GuaranteedHit) ? 1f : features.Contains(ItemFeature.Critical) ? 0.7f : CommonSenseParameters.SkillOnThrowProbabilityOfSuccess;

            var skillOnUse = SpawnEffectSkill.Build(
                new SkillData(
                    new AtFeet(),
                    area,
                    effectsOnUse,
                    repeat,
                    skillOnUseProbabilityOfSuccess,
                    "")
            );
            var skillOnThrow = SpawnEffectSkill.Build(
                new SkillData(
                    new AtFeet(),
                    new SelfArea(),
                    effectsOnThrow,
                    1,
                    skillOnThrowProbabilityOfSuccess,
                    "")
            );
            return (skillOnUse, skillOnThrow, hasSameEffect);
        }

        public static DirectWeaponMemento Build(DirectWeaponData data, WeaponPrefix? prefix = null, bool isCursed = false, ItemState state = ItemState.None, EnemyData? mimic = null)
        {
            var (skillOnUse, skillOnThrow, hasSameEffect) = BuildSkills(data.Power, 0, data.Features, prefix);
            var multiplyPrice = data.Features.Contains(ItemFeature.Artistic) ? 2f : 1f;
            var usageLossChance = 1 - data.Features.Count(f => f == ItemFeature.EnhanceDurability) * 0.2f;
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
                defaultPower: data.Power,
                features: data.Features,
                featureLimit: data.FeatureLimit,
                skillOnUse: skillOnUse,
                skillOnThrow: skillOnThrow,
                hasSameEffect: hasSameEffect
            ));
            var item = JsonUtility.FromJson<DirectWeaponMemento>(json); //MEMO: To break the sharing references
            return item;
        }

        private DirectWeapon Merge(IEnumerable<ItemFeature> featuresToMergeWeapon, int additionalUpgrade)
        {
            var memento = Serialize();
            var features = memento.Features.Merge(featuresToMergeWeapon, memento.FeatureLimit, FeatureApplicabilityTag.DirectWeapons).ToList();

            var (skillOnUse, skillOnThrow, hasSameEffect) = BuildSkills(memento.DefaultPower, memento.BaseItem.UpgradeCount + additionalUpgrade, features, memento.Prefix.Value);
            var multiplyPrice = features.Contains(ItemFeature.Artistic) ? 2f : 1f;
            var usageLossChance = 1 - features.Count(f => f == ItemFeature.EnhanceDurability) * 0.2f;
            var item = new DirectWeapon(memento.CopyWith(
                baseItem: memento.BaseItem.CopyWith(
                    multiplyPrice: multiplyPrice,
                    upgradeCount: memento.BaseItem.UpgradeCount + additionalUpgrade,
                    usageLossChance: usageLossChance
                ),
                features: features,
                skillOnUse: skillOnUse,
                skillOnThrow: skillOnThrow,
                hasSameEffect: hasSameEffect
            ));
            return item;
        }

        public DirectWeapon Merge(IItem mergedItem) => mergedItem.Match(
            item => Merge(item.FeaturesToMergeWeapon, item.UpgradeCount),
            directWeapon => Merge(directWeapon.Features, directWeapon.UpgradeCount),
            rangedWeapon => Merge(rangedWeapon.Features, rangedWeapon.UpgradeCount)
        );

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