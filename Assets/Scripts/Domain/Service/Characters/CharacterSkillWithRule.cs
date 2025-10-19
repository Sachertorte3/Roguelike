#nullable enable
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Memento;

namespace Domain.Service.Characters
{
    public class CharacterSkillWithRule : ICharacterSkillWithRule
    {
        public ICharacterSkill Skill { get; }
        public int Priority { get; }
        public CharacterSkillWithRule(CharacterSkillWithRuleMemento data)
        {
            Skill = new CharacterSkill(data.Skill);
            Priority = data.Priority;
        }
        public CharacterSkillWithRuleMemento Serialize()
        {
            return new CharacterSkillWithRuleMemento(
                Skill.Serialize(),
                Priority
            );
        }
        public static CharacterSkillWithRuleMemento Build(CharacterSkillWithRuleData data)
        {
            return new CharacterSkillWithRuleMemento(
                CharacterSkill.Build(data.Skill),
                data.Priority
            );
        }
    }
}