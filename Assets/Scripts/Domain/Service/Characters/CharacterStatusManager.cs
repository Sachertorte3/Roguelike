#nullable enable
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Characters;
using Domain.Model.Condition;
using Domain.Service.Characters.Conditions;
using Domain.Service.Characters.Stats;
using Domain.Service.Effect;
using ObservableCollections;
using R3;
using Stats;

namespace Domain.Service.Characters
{
    public class CharacterStatusManager : IDisposable, ISerializable<CharacterStatusMemento>, IStatusManager, ITarget
    {
        private readonly CharacterConditions _conditions;
        private readonly Subject<int> _onDamageReceived = new();
        private readonly Subject<int> _onHealReceived = new();
        private readonly CharacterStats _stats;
        private string _name;

        public CharacterStatusManager(string name, CharacterStatusMemento data)
        {
            _name = name;
            _stats = new CharacterStats(data.MaxHp, data.Hp);
            _conditions = new CharacterConditions(this, data.Conditions);
        }

        public Observable<Unit> OnDead => Stats.HpValue.Where(value => value <= 0).AsUnitObservable();
        public int CurrentMaxHp => _stats.MaxHp.CurrentValue;
        public int CurrentHp => _stats.Hp.Value.CurrentValue;

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
                _conditions.Conditions.Select(x => x.Serialize()).ToArray()
            );
        }

        public IStats Stats => _stats;
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

        public void UpdateTurn()
        {
            _conditions.UpdateTurn(this);
        }

        public void AddMaxHpValue(float value)
        {
            _stats.Hp.AddMaxHpValue(value);
        }

        public void AddMaxHpMultiplier(float value)
        {
            _stats.Hp.AddMaxHpMultiplier(value);
        }

        public void RemoveMaxHpValue(float value)
        {
            _stats.Hp.RemoveMaxHpValue(value);
        }

        public void RemoveMaxHpMultiplier(float value)
        {
            _stats.Hp.RemoveMaxHpMultiplier(value);
        }

        public static CharacterStatusMemento Build(int maxHp, int hp, bool isShiney)
        {
            if (isShiney)
            {
                return new CharacterStatusMemento(
                    maxHp * 3,
                    hp * 3,
                    new[] { Condition.Build(new Star(), new RemovalConditionData()) }
                );
            }
            else
            {
                return new CharacterStatusMemento(
                    maxHp,
                    hp,
                    new ConditionMemento[0]
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