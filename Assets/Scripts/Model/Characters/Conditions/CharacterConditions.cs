using Data.Condition;
using ObservableCollections;
using Sirenix.Utilities;
using System.Linq;
using R3;
using System;

namespace Model.Characters.Conditions
{
    internal class CharacterConditions : IDisposable
    {
        private readonly ObservableHashSet<Condition> Conditions = new();
        private readonly CompositeDisposable _disposables = new();
        public CharacterConditions(IHasCondition hasCondition)
        {
            _disposables.Add(Conditions.ObserveAdd().Subscribe(add => add.Value.Inflict(hasCondition)));
            _disposables.Add(Conditions.ObserveRemove().Subscribe(add => add.Value.Inflict(hasCondition)));
        }
        public void Dispose()
        {
            _disposables.Dispose();
        }
        public void Add(IConditionData condition, RemovalConditionData removalCondition)
        {
            Conditions.Add(new Condition(condition, removalCondition));
        }
        public void UpdateTurn(IHasCondition hasCondition)
        {
            Conditions.ForEach(condition => condition.UpdateTurn(hasCondition));
            Conditions.RemoveRange(Conditions.Where(condition => condition.ShouldDelete(0)).ToList());
        }
    }
    public class Condition
    {
        private int ElapsedTurn;
        private readonly IConditionData _condition;
        private readonly RemovalConditionData _removalCondition;
        public Condition(IConditionData condition, RemovalConditionData removalCondition)
        {
            ElapsedTurn = 0;
            _condition = condition;
            _removalCondition = removalCondition;
        }
        public void Inflict(IHasCondition hasCondition) => _condition.Inflict(hasCondition);
        public void Delete(IHasCondition hasCondition) => _condition.Delete(hasCondition);
        public void UpdateTurn(IHasCondition hasCondition)
        {
            ElapsedTurn += 1;
            _condition.Persist(hasCondition);
        }
        public bool ShouldDelete(int receivedDamage)
        {
            return _removalCondition.IsFinished(ElapsedTurn, receivedDamage);
        }
    }
}
