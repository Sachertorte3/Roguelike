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
        private readonly FlagStat _cannotActFlags;
        private readonly FlagStat _cannotMoveFlags;
        private readonly FlagStat _confusedFlags;
        private readonly FlagStat _overDriveFlags;
        private readonly FlagStat _hardFlags;
        private readonly FlagStat _heavyFlags;
        private readonly FlagStat _secureHoldFlags;
        private readonly FlagStat _curseProofFlags;
        private readonly FlagStat _isAffectedByTrapsFlags;
        public CharacterStatusManager(CharacterStatusMemento data, ReadOnlyReactiveProperty<Vector2Int> position,
            ICharacter character, IMap map)
        {
            _stats = new CharacterStats(data.Stats);
            _conditions = new CharacterConditions(character, data.Conditions, map.Player);
            _visionRange = new VisionRange(position, _stats.ViewRangeValue, data.ClairvoyantFlags, data.BlindFlags,
                character.CanThroughWalls, map);
            _cannotActFlags = new FlagStat(data.CannotActFlags);
            _cannotMoveFlags = new FlagStat(data.CannotMoveFlags);
            _confusedFlags = new FlagStat(data.ConfusedFlags);
            _overDriveFlags = new FlagStat(data.OverDriveFlags);
            _hardFlags = new FlagStat(data.HardFlags);
            _heavyFlags = new FlagStat(data.HeavyFlags);
            _secureHoldFlags = new FlagStat(data.SecureHoldFlags);
            _curseProofFlags = new FlagStat(data.CurseProofFlags);
            _isAffectedByTrapsFlags = new FlagStat(data.IsAffectedByTrapFlags);
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
                _cannotActFlags.CurrentFlags,
                _cannotMoveFlags.CurrentFlags,
                _confusedFlags.CurrentFlags,
                _visionRange.ClairvoyantFlags.CurrentFlags,
                _visionRange.BlindFlags.CurrentFlags,
                _overDriveFlags.CurrentFlags,
                _hardFlags.CurrentFlags,
                _heavyFlags.CurrentFlags,
                _secureHoldFlags.CurrentFlags,
                _curseProofFlags.CurrentFlags,
                _isAffectedByTrapsFlags.CurrentFlags,
                _conditions.ConditionsWithInflicter.Select(x => (x.actor, x.condition.Serialize())).ToList()
            );
        }

        public IStats Stats => _stats;
        public IVisionRange VisionRange => _visionRange;
        public IObservableCollection<ICondition> Conditions => _conditions.Conditions;
        public bool CannotAct => _cannotActFlags.CurrentValue;
        public bool CannotMove => _cannotMoveFlags.CurrentValue;
        public bool IsConfused => _confusedFlags.CurrentValue;
        public bool IsOverDrive => _overDriveFlags.CurrentValue;
        public bool IsHard => _hardFlags.CurrentValue;
        public bool IsHeavy => _heavyFlags.CurrentValue;
        public bool IsSecureHold => _secureHoldFlags.CurrentValue;
        public bool IsCurseProof => _curseProofFlags.CurrentValue;
        public ReadOnlyReactiveProperty<bool> IsAffectedByTraps => _isAffectedByTrapsFlags.Value;
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

        public void AddElementDamageRateMultiplier(Element element, float value)
        {
            _stats.AddElementDamageRateMultiplier(element, value);
        }

        public void RemoveElementAttackMultiplier(Element element, float value)
        {
            _stats.RemoveElementAttackMultiplier(element, value);
        }

        public void RemoveElementDamageRateMultiplier(Element element, float value)
        {
            _stats.RemoveElementDamageRateMultiplier(element, value);
        }

        private FlagStat GetFlagStat(FlagStatType type)
        {
            return type switch
            {
                FlagStatType.CannotAct => _cannotActFlags,
                FlagStatType.CannotMove => _cannotMoveFlags,
                FlagStatType.Clairvoyant => _visionRange.ClairvoyantFlags,
                FlagStatType.Blind => _visionRange.BlindFlags,
                FlagStatType.OverDrive => _overDriveFlags,
                FlagStatType.Hard => _hardFlags,
                FlagStatType.Heavy => _heavyFlags,
                FlagStatType.SecureHold => _secureHoldFlags,
                FlagStatType.CurseProof => _curseProofFlags,
                FlagStatType.IsAffectedByTrap => _isAffectedByTrapsFlags,
            };
        }

        public void AddFlagStat(FlagStatType type)
        {
            GetFlagStat(type).AddFlags();
        }

        public void RemoveFlagStat(FlagStatType type)
        {
            GetFlagStat(type).RemoveFlags();
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
            Dictionary<ConditionTemplate, float> conditionResistance, float viewRange, bool isHard, bool isHeavy, bool isAffectedByTrap, float waitTime, bool isSlept)
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
                stats: CharacterStats.Build(maxHp, hpNaturalRecoveryAmount, elementAttackMultiplier,
                    elementDamageRateMultiplier, conditionResistance, viewRange, waitTime),
                cannotActFlags: isSlept ? 1 : 0,
                cannotMoveFlags: 0,
                confusedFlags: 0,
                clairvoyantFlags: 0,
                blindFlags: isSlept ? 1 : 0,
                overDriveFlags: 0,
                hardFlags: isHard ? 1 : 0,
                heavyFlags: isHeavy ? 1 : 0,
                secureHoldFlags: 0,
                curseProofFlags: 0,
                isAffectedByTrapFlags: isAffectedByTrap ? 1 : 0,
                conditions: conditions
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