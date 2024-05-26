#nullable enable
using Cysharp.Threading.Tasks;
using Data.Condition;
using Model.Characters.Conditions;
using Model.Characters.Stats;
using Model.Logs;
using ObservableCollections;
using R3;
using System;
using System.Linq;

namespace Model.Characters
{
    public class CharacterStatusManager : IDisposable, IStatusManager
    {
        private readonly CharacterStats _stats;
        private readonly CharacterConditions _conditions;
        public CharacterStatusManager(int maxHp, int strength)
        {
            _stats = new CharacterStats(maxHp, strength);
            _conditions = new CharacterConditions(this);
        }
        public void Dispose()
        {
            _stats.Dispose();
            _conditions.Dispose();
        }
        public IStats Stats => _stats;
        public IObservableCollection<Condition> Conditions => _conditions.Conditions;
        public int MaxHp => _stats.MaxHp.CurrentValue;
        public int CurrentHp => _stats.Hp.Value.CurrentValue;
        public Observable<Unit> OnDead => Stats.HpValue.Where(value => value <= 0).AsUnitObservable();
        public bool IsDead => Stats.HpValue.CurrentValue <= 0;
        public UniTask GainHp(int value)
        {
            _stats.Hp.Gain(value);
            return UniTask.CompletedTask;
        }
        public UniTask LoseHp(int value)
        {
            _stats.Hp.Lose(value);
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
