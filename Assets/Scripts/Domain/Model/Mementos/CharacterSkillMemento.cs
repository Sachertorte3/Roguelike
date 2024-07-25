#nullable enable
namespace Domain.Model.Character
{
    public record CharacterSkillMemento(
        SkillMemento Skill,
        int CoolTime,
        int RemainingTurn
    );
}