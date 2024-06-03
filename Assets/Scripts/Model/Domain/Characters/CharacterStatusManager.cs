#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Data.Character;
using Data.Condition;
using Model.Domain.Characters.Conditions;
using Model.Domain.Characters.Stats;
using ObservableCollections;
using R3;

namespace Model.Domain.Characters
{
    public class CharacterStatusManager : IDisposable, ISerializable<CharacterStatusMemento>, IStatusManager
    {
        private string _name;
        private readonly CharacterConditions _conditions;
        private readonly CharacterStats _stats;

        public CharacterStatusManager(string name, int maxHp, int strength)
        {
            _name = name;
            _stats = new CharacterStats(maxHp, strength);
            _conditions = new CharacterConditions(this);
        }

        public CharacterStatusManager(string name, CharacterStatusMemento memento)
        {
            _name = name;
            _stats = new CharacterStats(memento.Hp, memento.Strength);
            _conditions = new CharacterConditions(this);
        }

        public CharacterStatusMemento Serialize()
        {
            return new CharacterStatusMemento(
                _stats.MaxHp.CurrentValue,
                _stats.Hp.Value.CurrentValue,
                _stats.Strength.CurrentValue
            );
        }

        public Observable<Unit> OnDead => Stats.HpValue.Where(value => value <= 0).AsUnitObservable();

        public void Dispose()
        {
            _stats.Dispose();
            _conditions.Dispose();
        }

        public IStats Stats => _stats;
        public IObservableCollection<Condition> Conditions => _conditions.Conditions;
        public int MaxHp => _stats.MaxHp.CurrentValue;
        public int CurrentHp => _stats.Hp.Value.CurrentValue;
        public bool IsDead => Stats.HpValue.CurrentValue <= 0;

        public UniTask GainHp(int value)
        {
            _stats.Hp.Gain(value, _name);
            return UniTask.CompletedTask;
        }

        public UniTask LoseHp(int value)
        {
            _stats.Hp.Lose(value, _name);
            return UniTask.CompletedTask;
        }

        public void AddCondition(IConditionData condition, RemovalConditionData removalCondition)
        {
            _conditions.Add(condition, removalCondition);
        }

        public void UpdateTurn()
        {
            _conditions.UpdateTurn(this);
        }
    }
}