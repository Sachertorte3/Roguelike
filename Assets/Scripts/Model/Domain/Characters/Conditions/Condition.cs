using Data.Character;
using Data.Condition;
using Utilities;

namespace Model.Domain.Characters.Conditions
{
    public class Condition : ISerializable<ConditionMemento>
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
            return new ConditionMemento(_condition, _removalCondition, _elapsedTurn);
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

        public bool ShouldDelete(int receivedDamage)
        {
            return _removalCondition.IsFinished(_elapsedTurn, receivedDamage);
        }
    }
}