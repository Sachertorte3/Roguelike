using Domain.Model.Condition;

namespace Domain.Model.Character
{
    public record ConditionMemento(
        IConditionData Condition,
        RemovalConditionData RemovalCondition,
        int ElapsedTurns
    );
}