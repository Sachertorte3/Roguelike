#nullable enable
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Conditions;
using Domain.Service.Characters.Stats;
using Domain.Service.Effect;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Stats;

namespace Domain.Service.Characters
{
    public class CharacterStatusManager : IDisposable, ISerializable<CharacterStatusMemento>, IStatusManager, ITarget
    {
        private readonly CharacterConditions _conditions;
        private readonly Subject<int> _onDamageReceived = new();
        private readonly Subject<int> _onHealReceived = new();
        private readonly CharacterStats _stats;
        private readonly VisionRange _visionRange;
        private readonly Dictionary<FlagStatType, FlagStat> _flagStats = new();

        public CharacterStatusManager(CharacterStatusMemento data, ReadOnlyReactiveProperty<Vector2Int> position,
            ICharacter character, IMap map)
        {
            _stats = new CharacterStats(data.Stats);
            _conditions = new CharacterConditions(character, data.Conditions, map);
            _flagStats = data.FlagStats.ToDictionary(x => x.Key, x => new FlagStat(x.Value));
            _visionRange = new VisionRange(position, _stats.ViewRangeValue, GetFlagStat(FlagStatType.Clairvoyant),
                GetFlagStat(FlagStatType.Blind), character.CanThroughWalls, map);
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
                _flagStats.ToDictionary(x => x.Key, x => x.Value.CurrentFlags),
                _conditions.ConditionsWithInflicter.Select(x => (x.actor, x.condition.Serialize())).ToList()
            );
        }

        public IStats Stats => _stats;
        public IVisionRange VisionRange => _visionRange;
        public IObservableCollection<ICondition> Conditions => _conditions.Conditions;

        public bool IsFlagStat(FlagStatType type)
        {
            return GetFlagStat(type).CurrentValue;
        }

        public ReadOnlyReactiveProperty<bool> GetFlagProperty(FlagStatType type)
        {
            return GetFlagStat(type).Value;
        }

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

        public void RestoreToFullHealth()
        {
            _stats.Hp.Set(Stats.CurrentMaxHp);
            _conditions.Clear();
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
            if (!_flagStats.TryGetValue(type, out var flagStat))
            {
                flagStat = new FlagStat(0);
                _flagStats[type] = flagStat;
            }

            return flagStat;
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
            Dictionary<ConditionTemplate, float> conditionResistance, float viewRange, bool isHard, bool isHeavy,
            bool isAffectedByTrap, float waitTime, bool isSlept)
        {
            var conditions = new List<(Id<IEntity> actor, ConditionMemento condition)>();
            if (isSlept)
            {
                conditions.Add(
                    (
                        Id<IEntity>.Empty,
                        Condition.Build(ScriptableObjectLoader.Load<ConditionTemplate>("まどろみ"))
                    )
                );
            }

            var flagStats = new Dictionary<FlagStatType, int>();
            if (isSlept)
            {
                flagStats[FlagStatType.CannotAct] = 1;
                flagStats[FlagStatType.Blind] = 1;
            }

            if (isHard)
            {
                flagStats[FlagStatType.Hard] = 1;
            }

            if (isHeavy)
            {
                flagStats[FlagStatType.Heavy] = 1;
            }

            if (isAffectedByTrap)
            {
                flagStats[FlagStatType.IsAffectedByTrap] = 1;
            }

            return new CharacterStatusMemento
            (
                CharacterStats.Build(maxHp, hpNaturalRecoveryAmount, elementAttackMultiplier,
                    elementDamageRateMultiplier, conditionResistance, viewRange, waitTime),
                flagStats,
                conditions
            );
        }

        public void AddCondition(Id<IEntity> actor, ConditionTemplate condition)
        {
            _conditions.Add(actor, condition);
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