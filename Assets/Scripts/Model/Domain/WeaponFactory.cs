using Data;
using Model.Domain.Effect;
using UnityEngine;

namespace Model.Domain
{

    public static class WeaponFactory
    {
        public static ItemData Create(MaterialData material, WeaponMold mold)
        {
            return new ItemData
            {
                Name = material.Name + mold.Name,
                Icon = mold.Icon,
                EffectsOnUse = true,
                EffectsOnThrow = true,
                Skill = new SkillData(
                    mold.Area,
                    new AttackEffect(Mathf.RoundToInt(material.Power * mold.PowerMagnification))
                ),
                UsageLimit = Mathf.RoundToInt(mold.UsageLimit * material.UsageLimitMagnification),
            };
        }
    }
}

