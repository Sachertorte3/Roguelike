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

        public CharacterStatusManager(CharacterStatusMemento data, ReadOnlyReactiveProperty<Vector2Int> position, IMap world)
        {
            _stats = new CharacterStats(data.Hp, data.HpNaturalRecoveryAmount, data.AttackMultiplier, data.ViewRange, data.WaitTime);
            _conditions = new CharacterConditions(this, data.Conditions);
            _visionRange = new VisionRange(position, _stats.ViewRangeValue, data.ClairvoyantFlags, world);
        }

        public Observable<Unit> OnDead => Stats.HpValue.Where(value => value <= 0).AsUnitObservable();

        public void Dispose()
        {
            _stats.Dispose();
            _conditions.Dispose();
        }

        public CharacterStatusMemento Serialize()
        {
            return new CharacterStatusMemento(
                _stats.Hp.GetData(),
                _stats.HpNaturalRecoveryAmount.GetData(),
                _stats.AttackMultiplier.GetData(),
                _stats.ViewRange.GetData(),
                _stats.WaitTime.GetData(),
                _visionRange.ClairvoyantFlags,
                _conditions.Conditions.Select(x => x.Serialize()).ToArray()
            );
        }

        public IStats Stats => _stats;
        public IVisionRange VisionRange => _visionRange;
        public IObservableCollection<ICondition> Conditions => _conditions.Conditions;
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
            GainHp(_stats.HpNaturalRecoveryAmount.CurrentValue, true);
            _conditions.UpdateTurn(this, enemyVisible);
        }

        public void AddMaxHpValue(float value)
        {
            _stats.Hp.AddMaxValue(value);
        }

        public void AddMaxHpMultiplier(float value)
        {
            _stats.Hp.AddMaxMultiplier(value);
        }

        public void AddHpNaturalRecoveryValue(float value)
        {
            _stats.HpNaturalRecoveryAmount.AddValue(value);
        }
        
        public void AddAttackMultiplierValue(float value)
        {
            _stats.AttackMultiplier.AddValue(value);
        }

        public void AddViewRangeMultiplier(float value)
        {
            _stats.ViewRange.AddMultiplier(value);
        }

        public void RemoveMaxHpValue(float value)
        {
            _stats.Hp.RemoveMaxValue(value);
        }

        public void RemoveMaxHpMultiplier(float value)
        {
            _stats.Hp.RemoveMaxMultiplier(value);
        }

        public void RemoveHpNaturalRecoveryValue(float value)
        {
            _stats.HpNaturalRecoveryAmount.AddValue(-value);
        }

        public void RemoveAttackMultiplierValue(float value)
        {
            _stats.AttackMultiplier.AddValue(-value);
        }

        public void RemoveViewRangeMultiplier(float value)
        {
            _stats.ViewRange.AddMultiplier(-value);
        }

        public void AddClairvoyantFlags()
        {
            _visionRange.AddClairvoyantFlags();
        }

        public void RemoveClairvoyantFlags()
        {
            _visionRange.RemoveClairvoyantFlags();
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

        public static CharacterStatusMemento Build(int maxHp, int hpNaturalRecoveryAmount, float viewRange, float waitTime, bool isSlept)
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
            return new CharacterStatusMemento(
                new ResourceData(new StatData(maxHp), maxHp),
                new StatData(hpNaturalRecoveryAmount),
                new StatData(1f),
                new StatData(viewRange),
                new ResourceData(new StatData(waitTime), 0),
                0,
                conditions.ToArray()
            );
        }

        public void AddCondition(IConditionData condition, RemovalConditionData removalCondition)
        {
            _conditions.Add(condition, removalCondition);
        }
    }
}