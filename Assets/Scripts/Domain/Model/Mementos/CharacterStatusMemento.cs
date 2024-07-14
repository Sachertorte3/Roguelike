using Sirenix.OdinInspector;
using Stats;

namespace Domain.Model.Character
{
    public record CharacterStatusMemento(
        ResourceData Hp,
        StatData HpNaturalRecoveryAmount,
        StatData ViewRange,
        int ClairvoyantFlags,
        ConditionMemento[] Conditions
    );
}