#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Conditions;
using Domain.Service.Characters.Stats;
using Domain.Service.Effect;
using ObservableCollections;
using R3;
using Stats;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters
{
    public class CharacterStatusManager : IDisposable, ISerializable<CharacterStatusMemento>, IStatusManager, ITarget
    {
        private readonly CharacterConditions _conditions;
        private readonly Subject<int> _onDamageReceived = new();
        private readonly Subject<int> _onHealReceived = new();
        private readonly CharacterStats _stats;
        private readonly VisionRange _visionRange;
        private readonly FlagStat _overDriveFlags;
        private readonly FlagStat _hardFlags;
        private readonly FlagStat _heavyFlags;
        public CharacterStatusManager(CharacterStatusMemento data, ReadOnlyReactiveProperty<Vector2Int> position,
            ICharacter character, IMap map)
        {
            _stats = new CharacterStats(data.Stats);
            _conditions = new CharacterConditions(character, data.Conditions, map.Player ?? character);
            _visionRange = new VisionRange(position, _stats.ViewRangeValue, data.ClairvoyantFlags, data.BlindFlags,
                character.CanThroughWalls, map);
            _overDriveFlags = new FlagStat(data.OverDriveFlags);
            _hardFlags = new FlagStat(data.HardFlags);
            _heavyFlags = new FlagStat(data.HeavyFlags);
        }

        public void Dispose()
        {
            _stats.Dispose();
            _conditions.Dispose();
        }

        public CharacterStatusMemento Serialize()
        {
            return new CharacterStatusMemento
            (
                _stats.Serialize(),
                _visionRange.ClairvoyantFlags,
                _visionRange.BlindFlags,
                _overDriveFlags.CurrentFlags,
                _hardFlags.CurrentFlags,
                _heavyFlags.CurrentFlags,
                _conditions.ConditionsWithInflicter.Select(x => (x.actor, x.condition.Serialize())).ToList()
            );
        }

        public IStats Stats => _stats;
        public IVisionRange VisionRange => _visionRange;
        public IObservableCollection<ICondition> Conditions => _conditions.Conditions;
        public bool IsOverDrive => _overDriveFlags.CurrentValue;
        public bool IsHard => _hardFlags.CurrentValue;
        public bool IsHeavy => _heavyFlags.CurrentValue;
        public bool IsDead => Stats.HpValue.CurrentValue <= 0;
        public Observable<int> OnDamageReceived => _onDamageReceived;
        public Observable<int> OnHealReceived => _onHealReceived;

        public int GainHp(float value, bool notifyOnlyActualGain = false)
        {
            var gainValue = _stats.Hp.Gain(value);
            if (notifyOnlyActualGain)
            {
                if (gainValue > 0)
                {
                    _onHealReceived.OnNext(gainValue);
                }
            }
            else
            {
                _onHealReceived.OnNext(Mathf.RoundToInt(value));
            }

            return gainValue;
        }

        public int LoseHp(float value, bool notifyOnlyActualLoss = false)
        {
            var loseValue = _stats.Hp.Lose(value);
            if (notifyOnlyActualLoss)
            {
                if (loseValue > 0)
                {
                    _onDamageReceived.OnNext(loseValue);
                }
            }
            else
            {
                _onDamageReceived.OnNext(Mathf.RoundToInt(value));
            }

            return loseValue;
        }

        public void UpdateTurn(IHasCondition hasCondition, bool characterVisible)
        {
            if (_stats.HpNaturalRecoveryAmount.CurrentValue > 0)
                GainHp(_stats.HpNaturalRecoveryAmount.CurrentValue, true);
            else
                LoseHp(-_stats.HpNaturalRecoveryAmount.CurrentValue, true);
            _conditions.UpdateTurn(hasCondition, characterVisible);
        }

        public void WasAttacked()
        {
            _conditions.WasAttacked();
        }

        public float GetStatValue(StatType type)
        {
            return _stats.GetStatValue(type);
        }

        public void AddStatValue(StatType type, float value)
        {
            _stats.AddStatValue(type, value);
        }

        public void RemoveStatValue(StatType type, float value)
        {
            _stats.RemoveStatValue(type, value);
        }

        public void AddStatMultiplier(StatType type, float value)
        {
            _stats.AddStatMultiplier(type, value);
        }

        public void RemoveStatMultiplier(StatType type, float value)
        {
            _stats.RemoveStatMultiplier(type, value);
        }

        public void MultiplyStat(StatType type, float value)
        {
            _stats.MultiplyStat(type, value);
        }

        public void DivideStat(StatType type, float value)
        {
            _stats.DivideStat(type, value);
        }

        public void AddElementAttackMultiplier(Element element, float value)
        {
            _stats.AddElementAttackMultiplier(element, value);
        }

        public void RemoveElementAttackMultiplier(Element element, float value)
        {
            _stats.RemoveElementAttackMultiplier(element, value);
        }

        public void AddFlagStat(FlagStatType type)
        {
            switch (type)
            {
                case FlagStatType.Clairvoyant:
                    _visionRange.AddClairvoyantFlags();
                    break;
                case FlagStatType.Blind:
                    _visionRange.AddBlindFlags();
                    break;
                case FlagStatType.OverDrive:
                    _overDriveFlags.AddFlags();
                    break;
                case FlagStatType.Hard:
                    _hardFlags.AddFlags();
                    break;
                case FlagStatType.Heavy:
                    _heavyFlags.AddFlags();
                    break;
            }
        }

        public void RemoveFlagStat(FlagStatType type)
        {
            switch (type)
            {
                case FlagStatType.Clairvoyant:
                    _visionRange.RemoveClairvoyantFlags();
                    break;
                case FlagStatType.Blind:
                    _visionRange.RemoveBlindFlags();
                    break;
                case FlagStatType.OverDrive:
                    _overDriveFlags.RemoveFlags();
                    break;
                case FlagStatType.Hard:
                    _hardFlags.RemoveFlags();
                    break;
                case FlagStatType.Heavy:
                    _heavyFlags.RemoveFlags();
                    break;
            }
        }

        public void AddWaitTime(float value)
        {
            _stats.WaitTime.Gain(value);
        }

        public void ResetWaitTime()
        {
            _stats.WaitTime.Set(0);
        }

        public bool IsWaitTimeFull()
        {
            return _stats.WaitTime.IsFull();
        }

        public static CharacterStatusMemento Build(int maxHp, float hpNaturalRecoveryAmount,
            Dictionary<Element, float> elementAttackMultiplier, Dictionary<Element, float> elementDamageRateMultiplier,
            Dictionary<ConditionTemplate, float> conditionResistance, float viewRange, bool isHard, bool isHeavy, float waitTime, bool isSlept)
        {
            var conditions = new List<(Id<IEntity> actor, ConditionMemento condition)>();
            if (isSlept)
            {
                conditions.Add(
                    (
                        Id<IEntity>.Empty,
                        Condition.Build(
                            new Slept(),
                            new RemovalConditionData(damageProbability: 0.75f, characterNearbyProbability: 0.5f)
                        )
                    )
                );
            }

            return new CharacterStatusMemento
            (
                CharacterStats.Build(maxHp, hpNaturalRecoveryAmount, elementAttackMultiplier,
                    elementDamageRateMultiplier, conditionResistance, viewRange, waitTime),
                0,
                isSlept ? 1 : 0,
                0,
                isHard ? 1 : 0,
                isHeavy ? 1 : 0,
                conditions
            );
        }

        public void AddCondition(Id<IEntity> actor, IConditionData condition, RemovalConditionData removalCondition)
        {
            _conditions.Add(actor, condition, removalCondition);
        }

        public void RemoveConditionType(Type conditionType)
        {
            _conditions.RemoveType(conditionType);
        }

        public void ClearCondition()
        {
            _conditions.Clear();
        }
    }
}