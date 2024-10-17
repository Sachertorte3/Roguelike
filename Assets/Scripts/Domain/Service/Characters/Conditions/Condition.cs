using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Memento;
using Domain.Service.Logs;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Condition : ICondition
    {
        private readonly IConditionData _condition;
        private readonly RemovalConditionData _removalCondition;
        private int _elapsedTurn;

        public Condition(ConditionMemento memento)
        {
            _elapsedTurn = memento.ElapsedTurns;
            _condition = memento.Condition;
            _removalCondition = memento.RemovalCondition;
        }

        public ParticleType ParticleType => _condition.ParticleType;

        public ConditionMemento Serialize()
        {
            return new ConditionMemento
            (
                _condition,
                _removalCondition,
                _elapsedTurn
            );
        }

        public ConditionMemento Build(IConditionData condition, RemovalConditionData removalCondition,
            int elapsedTurn = 0)
        {
            return new ConditionMemento
            (
                condition,
                removalCondition,
                elapsedTurn
            );
        }

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor, IHasAffiliation player)
        {
            if (_condition.InflictLog != "")
            {
                GameLog.Add($"{hasCondition.GetName(player)}{_condition.InflictLog}");
            }
            _condition.Inflict(hasCondition, actor);
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor, IHasAffiliation player)
        {
            if (_condition.DeleteLog != "")
            {
                GameLog.Add($"{hasCondition.GetName(player)}{_condition.DeleteLog}");
            }
            _condition.Delete(hasCondition, actor);
        }

        public void UpdateTurn(IHasCondition hasCondition)
        {
            _elapsedTurn += 1;
            _condition.Persist(hasCondition);
        }

        public bool ShouldDelete(bool characterVisible)
        {
            return _removalCondition.IsFinished(_elapsedTurn, characterVisible);
        }

        public bool ShouldDeleteByDamage()
        {
            return _removalCondition.IsFinishedByDamage();
        }

        public bool EqualsConditionType(System.Type conditionType)
        {
            return _condition.GetType() == conditionType;
        }

        public static ConditionMemento Build(IConditionData condition, RemovalConditionData removalCondition)
        {
            return new ConditionMemento
            (
                condition,
                removalCondition,
                0
            );
        }
    }
}