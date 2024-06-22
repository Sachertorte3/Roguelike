#nullable enable
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data.Character;
using Data.Condition;
using Model.Domain.Characters.Conditions;
using Model.Domain.Characters.Stats;
using Model.Domain.Effect;
using ObservableCollections;
using R3;

namespace Model.Domain.Characters
{
    public class CharacterStatusManager : IDisposable, ISerializable<CharacterStatusMemento>, IStatusManager, ITarget
    {
        private readonly CharacterConditions _conditions;
        private readonly Subject<int> _onDamageReceived = new();
        private readonly Subject<int> _onHealReceived = new();
        private readonly CharacterStats _stats;
        private string _name;

        public CharacterStatusManager(string name, CharacterStatusMemento memento)
        {
            _name = name;
            _stats = new CharacterStats(memento.MaxHp, memento.Hp);
            _conditions = new CharacterConditions(this);
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
        public IObservableCollection<Condition> Conditions => _conditions.Conditions;
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

        public static CharacterStatusMemento Build(int maxHp, int hp)
        {
            return new CharacterStatusMemento(maxHp, hp, new ConditionMemento[0]);
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