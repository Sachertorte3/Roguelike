using Data.Condition;
using ObservableCollections;
using System.Linq;
using R3;
using System;
using Utilities;

namespace Model.Characters.Conditions
{
    internal class CharacterConditions : IDisposable, ICharacterConditions
    {
        public Observable<Condition> OnConditionAdded => _conditions.ObserveAdd().Select(add => add.Value);
        public Observable<Condition> OnConditionRemoved => _conditions.ObserveRemove().Select(remove => remove.Value);
        private readonly ObservableHashSet<Condition> _conditions = new();
        private readonly CompositeDisposable _disposables = new();
        public CharacterConditions(IHasCondition hasCondition)
        {
            _disposables.Add(_conditions.ObserveAdd().Subscribe(add => add.Value.Inflict(hasCondition)));
            _disposables.Add(_conditions.ObserveRemove().Subscribe(add => add.Value.Inflict(hasCondition)));
        }
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
    public interface ICharacterConditions
    {
        public Observable<Condition> OnConditionAdded { get; }
        public Observable<Condition> OnConditionRemoved { get; }
    }
    public class Condition
    {
        private int ElapsedTurn;
        private readonly IConditionData _condition;
        public ParticleType ParticleType => _condition.ParticleType;
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
