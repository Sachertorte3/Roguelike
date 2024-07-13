#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Characters;
using Domain.Model.Condition;
using Domain.Model.Effect;
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
        private string _name;

        public CharacterStatusManager(string name, CharacterStatusMemento data, ReadOnlyReactiveProperty<Vector2Int> position, IMap world)
        {
            _name = name;
            _stats = new CharacterStats(data.MaxHp, data.Hp, data.ViewRange);
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
                _stats.MaxHp.CurrentValue,
                _stats.Hp.Value.CurrentValue,
                _stats.ViewRange.CurrentValue,
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

        public UniTask<int> LoseHp(int value)
        {
            var loseValue = _stats.Hp.Lose(value, _name);
            _onDamageReceived.OnNext(value);
            return UniTask.FromResult(loseValue);
        }

        public void UpdateTurn(bool enemyVisible)
        {
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

        public static CharacterStatusMemento Build(int maxHp, int hp, float viewRange, bool isSleeped, bool isShiney)
        {
            var conditions = new List<ConditionMemento>();
            if (isSleeped)
            {
                conditions.Add(Condition.Build(new Sleeped(), new RemovalConditionData(acceptableDamage: 0, probability: 0.5f, removeByEnemyNearby: true)));
            }
            if (isShiney)
            {
                conditions.Add(Condition.Build(new Star(), new RemovalConditionData()));
                return new CharacterStatusMemento(
                    maxHp * 3,
                    hp * 3,
                    viewRange,
                    0,
                    conditions.ToArray()
                );
            }
            else
            {
                return new CharacterStatusMemento(
                    maxHp,
                    hp,
                    viewRange,
                    0,
                    conditions.ToArray()
                );
            }

        }

        public UniTask<int> GainHp(int value)
        {
            var gainValue = _stats.Hp.Gain(value, _name);
            _onHealReceived.OnNext(value);
            return UniTask.FromResult(gainValue);
        }

        public void AddCondition(IConditionData condition, RemovalConditionData removalCondition)
        {
            _conditions.Add(condition, removalCondition);
        }
    }
}