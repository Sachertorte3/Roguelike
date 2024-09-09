using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Memento;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Condition : ICondition
    {
        private readonly IConditionData _condition;
        private readonly RemovalConditionData _removalCondition;
        private int _elapsedTurn;

        public Condition(IConditionData condition, RemovalConditionData removalCondition, int elapsedTurn = 0)
        {
            _elapsedTurn = elapsedTurn;
            _condition = condition;
            _removalCondition = removalCondition;
        }

        public ParticleType ParticleType => _condition.ParticleType;
        public bool CanAct => _condition.CanAct;
        public bool CausesConfusion => _condition.CausesConfusion;

        public ConditionMemento Serialize()
        {
            return new ConditionMemento
            {
                Condition = _condition,
                RemovalCondition = _removalCondition,
                ElapsedTurns = _elapsedTurn
            };
        }

        public void Inflict(IHasCondition hasCondition)
        {
            _condition.Inflict(hasCondition);
        }

        public void Delete(IHasCondition hasCondition)
        {
            _condition.Delete(hasCondition);
        }

        public void UpdateTurn(IHasCondition hasCondition)
        {
            _elapsedTurn += 1;
            _condition.Persist(hasCondition);
        }

        public bool ShouldDelete(bool enemyVisible)
        {
            return _removalCondition.IsFinished(_elapsedTurn, enemyVisible);
        }

        public bool ShouldDeleteByDamage()
        {
            return _removalCondition.IsFinishedByDamage();
        }

        public static ConditionMemento Build(IConditionData condition, RemovalConditionData removalCondition)
        {
            return new ConditionMemento
            {
                Condition = condition,
                RemovalCondition = removalCondition,
                ElapsedTurns = 0
            };
        }
    }
}