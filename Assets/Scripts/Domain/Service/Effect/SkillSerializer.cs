#nullable enable
using Domain.Model.Character;
using Domain.Model.Effect;
using System;

namespace Domain.Service.Effect
{
    public static class SkillSerializer
    {
        public static ISkillMemento Serialize(this ISkill skill)
        {
            return skill switch
            {
                SpawnEffectSkill spawnEffectSkill => spawnEffectSkill.Serialize(),
                ItemTargetSkill itemTargetSkill => itemTargetSkill.Serialize(),
                _ => throw new ArgumentException("Invalid skill type")
            };
        }
        public static ISkill Deserialize(this ISkillMemento memento)
        {
            return memento switch
            {
                SkillMemento skillMemento => new SpawnEffectSkill(skillMemento),
                ItemTargetSkillMemento itemTargetSkillMemento => new ItemTargetSkill(itemTargetSkillMemento),
                _ => throw new ArgumentException("Invalid memento type")
            };
        }
    }
}