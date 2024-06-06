using Data.Condition;
using Utilities;

namespace Model.Domain.Characters.Conditions
{
    public class Condition
    {
        private readonly IConditionData _condition;
        private readonly RemovalConditionData _removalCondition;
        private int ElapsedTurn;

        public Condition(IConditionData condition, RemovalConditionData removalCondition)
        {
            ElapsedTurn = 0;
            _condition = condition;
            _removalCondition = removalCondition;
        }

        public ParticleType ParticleType => _condition.ParticleType;
        public bool CanAct => _condition.CanAct;

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
            ElapsedTurn += 1;
            _condition.Persist(hasCondition);
        }

        public bool ShouldDelete(int receivedDamage)
        {
            return _removalCondition.IsFinished(ElapsedTurn, receivedDamage);
        }
    }
}