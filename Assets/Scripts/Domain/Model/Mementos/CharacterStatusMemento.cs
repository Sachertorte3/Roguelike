namespace Domain.Model.Character
{
    public record CharacterStatusMemento(
        int MaxHp,
        int Hp,
        ConditionMemento[] Conditions
    );
}