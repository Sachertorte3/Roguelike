using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Memento;
using ObservableCollections;
using R3;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class CharacterConditions : IDisposable
    {
        private readonly ObservableHashSet<ICondition> _conditions = new();
        private readonly Dictionary<ICondition, Id<IEntity>> _inflicterMap = new();
        private readonly CompositeDisposable _disposables = new();

        public CharacterConditions(IHasCondition hasCondition,
            List<(Id<IEntity> actor, ConditionMemento condition)> conditions, IHasAffiliation player)
        {
            foreach (var (actor, conditionMemento) in conditions)
            {
                var condition = new Condition(conditionMemento);
                _conditions.Add(condition);
                _inflicterMap.Add(condition, actor);
            }

            _disposables.Add(_conditions.ObserveAdd()
                .Subscribe(add => add.Value.Inflict(hasCondition, _inflicterMap[add.Value], player)));
            _disposables.Add(_conditions.ObserveRemove()
                .Subscribe(remove => remove.Value.Delete(hasCondition, _inflicterMap[remove.Value], player)));
        }

        public IObservableCollection<ICondition> Conditions => _conditions;

        public List<(Id<IEntity> actor, ICondition condition)> ConditionsWithInflicter =>
            _conditions.Select(condition => (_inflicterMap[condition], condition)).ToList();

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Add(Id<IEntity> actor, IConditionData conditionData, RemovalConditionData removalCondition)
        {
            var condition = new Condition(Condition.Build(conditionData, removalCondition));
            _inflicterMap.Add(condition, actor);
            _conditions.Add(condition);
        }

        public void Clear()
        {
            foreach (var condition in _conditions.ToList())
            {
                _conditions.Remove(condition);
                _inflicterMap.Remove(condition);
            }
        }

        public void UpdateTurn(IHasCondition hasCondition, bool characterVisible)
        {
            _conditions.RemoveRange(_conditions.Where(condition => condition.ShouldDelete(characterVisible)).ToList());
            _conditions.ForEach(condition => condition.UpdateTurn(hasCondition));
        }

        public void WasAttacked()
        {
            _conditions.RemoveRange(_conditions.Where(condition => condition.ShouldDeleteByDamage()).ToList());
        }
    }
}