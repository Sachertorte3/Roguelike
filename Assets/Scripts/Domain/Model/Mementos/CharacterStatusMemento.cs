using Sirenix.OdinInspector;

namespace Domain.Model.Character
{
    public record CharacterStatusMemento(
        int MaxHp,
        int Hp,
        float ViewRange,
        ConditionMemento[] Conditions
    );
}