#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Service.Characters.Conditions;
using Domain.Service.Characters.Stats;
using Domain.Service.Effect;
using ObservableCollections;
using R3;
using Stats;
using UnityEngine;

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

        public CharacterStatusManager(CharacterStatusMemento data, ReadOnlyReactiveProperty<Vector2Int> position, IMap world)
        {
            _stats = new CharacterStats(data.Stats);
            _conditions = new CharacterConditions(this, data.Conditions);
            _visionRange = new VisionRange(position, _stats.ViewRangeValue, data.ClairvoyantFlags, world);
            _overDriveFlags = new FlagStat(data.OverDriveFlags);
        }

        public void Dispose()
        {
            _stats.Dispose();
            _conditions.Dispose();
        }

        public CharacterStatusMemento Serialize()
        {
            return new CharacterStatusMemento
            {
                Stats = _stats.Serialize(),
                ClairvoyantFlags = _visionRange.ClairvoyantFlags,
                OverDriveFlags = _overDriveFlags.CurrentFlags,
                Conditions = _conditions.Conditions.Select(x => x.Serialize()).ToArray()
            };
        }

        public IStats Stats => _stats;
        public IVisionRange VisionRange => _visionRange;
        public IObservableCollection<ICondition> Conditions => _conditions.Conditions;
        public bool IsOverDrive => _overDriveFlags.CurrentValue;
        public bool IsDead => Stats.HpValue.CurrentValue <= 0;
        public Observable<int> OnDamageReceived => _onDamageReceived;
        public Observable<int> OnHealReceived => _onHealReceived;

        public int GainHp(int value, bool notifyOnlyActualGain = false)
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
                _onHealReceived.OnNext(value);
            }
            return gainValue;
        }

        public int LoseHp(int value, bool notifyOnlyActualLoss = false)
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
                _onDamageReceived.OnNext(value);
            }
            return loseValue;
        }

        public void UpdateTurn(bool enemyVisible)
        {
            if (_stats.HpNaturalRecoveryAmount.CurrentValue > 0)
                GainHp(_stats.HpNaturalRecoveryAmount.CurrentValue, true);
            else
                LoseHp(-_stats.HpNaturalRecoveryAmount.CurrentValue, true);
            _conditions.UpdateTurn(this, enemyVisible);
        }

        public float GetStatValue(StatType type) => _stats.GetStatValue(type);
        public void AddStatValue(StatType type, float value) => _stats.AddStatValue(type, value);
        public void RemoveStatValue(StatType type, float value) => _stats.RemoveStatValue(type, value);
        public void AddStatMultiplier(StatType type, float value) => _stats.AddStatMultiplier(type, value);
        public void RemoveStatMultiplier(StatType type, float value) => _stats.RemoveStatMultiplier(type, value);
        public void MultiplyStat(StatType type, float value) => _stats.MultiplyStat(type, value);
        public void DivideStat(StatType type, float value) => _stats.DivideStat(type, value);
        public void AddElementAttackMultiplier(Element element, float value) => _stats.AddElementAttackMultiplier(element, value);
        public void RemoveElementAttackMultiplier(Element element, float value) => _stats.RemoveElementAttackMultiplier(element, value);
        public void AddClairvoyantFlags()
        {
            _visionRange.AddClairvoyantFlags();
        }
        public void RemoveClairvoyantFlags()
        {
            _visionRange.RemoveClairvoyantFlags();
        }
        public void AddOverDriveFlags()
        {
            _overDriveFlags.AddFlags();
        }
        public void RemoveOverDriveFlags()
        {
            _overDriveFlags.RemoveFlags();
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

        public static CharacterStatusMemento Build(int maxHp, int hpNaturalRecoveryAmount, float attackMultiplier, Dictionary<Element, float> elementAttackMultiplier, Dictionary<Element, float> elementDamageRateMultiplier, float viewRange, float waitTime, bool isSlept)
        {
            var conditions = new List<ConditionMemento>();
            if (isSlept)
            {
                conditions.Add(
                    Condition.Build(
                        new Slept(),
                        new RemovalConditionData(probability: 0.5f, removeByEnemyNearby: true)
                    )
                );
            }
            return new CharacterStatusMemento
            {
                Stats = CharacterStats.Build(maxHp, hpNaturalRecoveryAmount, attackMultiplier, elementAttackMultiplier, elementDamageRateMultiplier, viewRange, waitTime, isSlept),
                ClairvoyantFlags = 0,
                Conditions = conditions.ToArray()
            };
        }

        public void AddCondition(IConditionData condition, RemovalConditionData removalCondition)
        {
            _conditions.Add(condition, removalCondition);
        }
    }
}