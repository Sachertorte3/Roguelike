using System;
using Domain.Model.Effect;
using Sirenix.OdinInspector;

namespace Domain.Model.Character
{
    [Serializable]
    public class EnemySkillData
    {
        [Required] public SkillData Skill;
        [MinValue(0)] public int CoolTime;
    }
}