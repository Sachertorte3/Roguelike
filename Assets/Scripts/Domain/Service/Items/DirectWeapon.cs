#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
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
using UnityEngine.UIElements;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public class DirectWeapon : BaseItem, ISerializable<DirectWeaponMemento>
    {
        private const ItemCategory _category = ItemCategory.Weapons;
        private const bool _hasSameEffect = true;
        private const bool _hasSameSkill = false;
        private const bool _useOnDeath = false;
        private const StorageMemento? _storage = null;
        private const bool _cannotUseIfCursed = false;
        private const bool _cannotDropIfCursed = true;
        private const bool _identifyIfGot = true;
        private const bool _identifyIfUsed = true;
        private const bool _autoDestroyWhenDisabled = false;
        private Option<WeaponPrefix> _prefix { get; init; }
        private List<ElementPower> _elementPowers { get; init; }
        private List<DirectWeaponFeature> _features { get; init; }
        private SpawnEffectSkill _skillOnUse { get; init; }
        private SpawnEffectSkill _skillOnThrow { get; init; }
        public override Option<ISkill> SkillOnUse => ((ISkill)_skillOnUse).ToOption();
        public override Option<ISkill> SkillOnThrow => ((ISkill)_skillOnThrow).ToOption();
        public override string RevealedName => _prefix.MapOr("", prefix => prefix.Name) + BaseName;
        public DirectWeapon(DirectWeaponData data) : this(Build(data))
        {
        }

        public DirectWeapon(DirectWeaponMemento data) : base(
            data.Id, _category, data.BaseName, data.CustomName, data.Icon,
            data.IsShiny, data.State, data.UpgradePaths, _hasSameEffect, _hasSameSkill,
            _useOnDeath, _storage.ToOption(), data.MaxUsages, data.RemainingUsages, data.IsCursed,
            _cannotUseIfCursed, _cannotDropIfCursed, _identifyIfGot, _identifyIfUsed,
            data.IsCurseIdentified, _autoDestroyWhenDisabled, data.UpgradeLimit, data.Conditions.ToList())
        {
            _prefix = data.Prefix;
            _elementPowers = data.ElementPowers;
            _features = data.Features;
            _skillOnUse = new SpawnEffectSkill(data.SkillOnUse);
            _skillOnThrow = new SpawnEffectSkill(data.SkillOnThrow);
        }

        public DirectWeaponMemento Serialize()
        {
            var json = JsonUtility.ToJson(new DirectWeaponMemento
            (
                id: Id,
                baseName: BaseName,
                revealedName: RevealedName,
                customName: CustomName,
                icon: Icon,
                isShiny: IsShiny,
                state: State,
                upgradePaths: _upgradePaths.Select(path => path.ToString()).ToList(),
                prefix: _prefix,
                elementPowers: _elementPowers,
                features: _features,
                skillOnUse: _skillOnUse.Serialize(),
                skillOnThrow: _skillOnThrow.Serialize(),
                maxUsages: MaxUsages,
                remainingUsages: RemainingUses.CurrentValue,
                isCursed: IsCursed,
                isCurseIdentified: IsCurseIdentified,
                upgradeLimit: UpgradeLimit,
                conditions: _conditions
            ));
            return JsonUtility.FromJson<DirectWeaponMemento>(json);
        }

        public DirectWeaponMemento SerializeIgnoreUpgrades()
        {
            foreach (var upgradePath in _upgradePaths)
            {
                this.ApplyDowngrade(upgradePath);
            }
            var json = JsonUtility.ToJson(new DirectWeaponMemento
            (
                id: Id,
                baseName: BaseName,
                revealedName: RevealedName,
                customName: CustomName,
                icon: Icon,
                isShiny: IsShiny,
                state: State,
                upgradePaths: _upgradePaths.Select(path => path.ToString()).ToList(),
                prefix: _prefix,
                elementPowers: _elementPowers,
                features: _features,
                skillOnUse: _skillOnUse.Serialize(),
                skillOnThrow: _skillOnThrow.Serialize(),
                maxUsages: MaxUsages,
                remainingUsages: RemainingUses.CurrentValue,
                isCursed: IsCursed,
                isCurseIdentified: IsCurseIdentified,
                upgradeLimit: UpgradeLimit,
                conditions: _conditions
            ));
            foreach (var upgradePath in _upgradePaths)
            {
                this.ApplyUpgrade(upgradePath);
            }
            return JsonUtility.FromJson<DirectWeaponMemento>(json);
        }

        public static (SpawnEffectSkill skillOnUse, SpawnEffectSkill skillOnThrow) BuildSkills(List<ElementPower> elementPowers, List<DirectWeaponFeature> features, WeaponPrefix? prefix = null, bool skipMultiplyPower = false)
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
            var criticalRate = features.Contains(DirectWeaponFeature.Critical) ? 0.25f : 0f;
            var throwEnhance = features.Contains(DirectWeaponFeature.ThrowEnhance) ? 1.5f : 1f;
            if (features.Contains(DirectWeaponFeature.Absorbing))
            {
                effectsOnUse.Add(new AbsorbsEffect(
                    elementPowers,
                    0.25f
                ));
                effectsOnThrow.Add(new AbsorbsEffect(
                    elementPowers.Select(power => power.MultiplyPower(throwEnhance)).ToList(),
                    0.25f
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

            if (prefix != null)
            {
                foreach (var effect in prefix.AdditionalEffects)
                {
                    effectsOnUse.Add(effect);
                    effectsOnThrow.Add(effect);
                }
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
                    "は斬りかかった")
            ));
            var skillOnThrow = new SpawnEffectSkill(SpawnEffectSkill.Build(
                new SkillData(
                    new AtFeet(),
                    new SelfArea(),
                    effectsOnThrow,
                    1,
                    skillOnThrowProbabilityOfSuccess,
                    "は斬りかかった")
            ));
            return (skillOnUse, skillOnThrow);
        }

        public static DirectWeaponMemento Build(DirectWeaponData data, WeaponPrefix? prefix = null, bool isCursed = false, ItemState state = ItemState.None)
        {
            var (skillOnUse, skillOnThrow) = BuildSkills(data.ElementPowers, data.Features, prefix);

            var maxUsages = Mathf.RoundToInt(data.UsageLimit * prefix.ToOption().MapOr(1, prefix => prefix.UsageLimitMagnification));

            var json = JsonUtility.ToJson(new DirectWeaponMemento
            (
                id: Id<IItem>.Generate(),
                baseName: data.name,
                revealedName: data.name,
                customName: Option<string>.None,
                icon: data.Icon,
                isShiny: data.IsShiny,
                state: state,
                upgradePaths: new List<string>(),
                prefix: prefix.ToOption(),
                elementPowers: data.ElementPowers,
                features: data.Features,
                skillOnUse: skillOnUse.Serialize(),
                skillOnThrow: skillOnThrow.Serialize(),
                maxUsages: maxUsages,
                remainingUsages: maxUsages,
                isCursed: isCursed,
                isCurseIdentified: false,
                upgradeLimit: data.UpgradeLimit + prefix.ToOption().MapOr(0, prefix => prefix.AdditionalUpgradeLimit),
                conditions: data.PassiveConditions
            ));
            var item = JsonUtility.FromJson<DirectWeaponMemento>(json); //MEMO: To break the sharing references
            return item;
        }

        public DirectWeapon Merge(DirectWeapon item)
        {
            //MEMO: There is also a way to reload the data and regenerate it from scratch.
            var memento = SerializeIgnoreUpgrades();
            var features = memento.Features.Concat(item._features).ToList();
            var (skillOnUse, skillOnThrow) = BuildSkills(memento.ElementPowers, features, memento.Prefix.Value, true);
            var mergedItem = new DirectWeapon(memento.CopyWith(
                features: features,
                skillOnUse: skillOnUse.Serialize(),
                skillOnThrow: skillOnThrow.Serialize()
            ));
            foreach (var upgradePath in mergedItem._upgradePaths)
            {
                mergedItem.ApplyUpgrade(upgradePath);
            }
            foreach (var upgradePath in item._upgradePaths.Shuffled())
            {
                if (mergedItem.CanUpgrade(upgradePath.ToString()))
                {
                    mergedItem.UpgradeNoLog(upgradePath);
                }
            }
            return mergedItem;
        }
    }
}