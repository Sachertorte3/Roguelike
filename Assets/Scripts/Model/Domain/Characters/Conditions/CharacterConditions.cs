using System;
using System.Collections.Generic;
using System.Linq;
using Data.Character;
using Data.Condition;
using ObservableCollections;
using R3;
using Utilities;

namespace Model.Domain.Characters.Conditions
{
    internal class CharacterConditions : IDisposable
    {
        private readonly ObservableHashSet<Condition> _conditions = new();
        private readonly CompositeDisposable _disposables = new();

        public CharacterConditions(IHasCondition hasCondition, ConditionMemento[] conditions)
        {
            foreach (var condition in conditions)
            {
                _conditions.Add(new Condition(condition.Condition, condition.RemovalCondition, condition.ElapsedTurns));
            }
            _disposables.Add(_conditions.ObserveAdd().Subscribe(add => add.Value.Inflict(hasCondition)));
            _disposables.Add(_conditions.ObserveRemove().Subscribe(add => add.Value.Delete(hasCondition)));
        }

        public IObservableCollection<Condition> Conditions => _conditions;

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Add(IConditionData condition, RemovalConditionData removalCondition)
        {
            _conditions.Add(new Condition(condition, removalCondition));
        }

        public void UpdateTurn(IHasCondition hasCondition)
        {
            _conditions.ForEach(condition => condition.UpdateTurn(hasCondition));
            _conditions.RemoveRange(_conditions.Where(condition => condition.ShouldDelete(0)).ToList());
        }
    }
}