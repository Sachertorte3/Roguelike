using Data;
using Data.Area;
using Effect.Position;
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
                SkillOnUse = new SkillDataOnUse(
                    new AtFeet(),
                    mold.Area,
                    new AttackEffect(Mathf.RoundToInt(material.Power * mold.PowerMagnification), new())
                ),
                SkillOnThrow = new SkillDataOnThrow(
                    new SelfArea(),
                    new AttackEffect(Mathf.RoundToInt(material.Power * mold.PowerMagnification), new())
                ),
                UsageLimit = Mathf.RoundToInt(mold.UsageLimit * material.UsageLimitMagnification),
            };
        }
        public static ItemData Create(WeaponPrefix prefix, MaterialData material, WeaponMold mold)
        {
            return new ItemData
            {
                Name = prefix.Name + material.Name + mold.Name,
                Icon = mold.Icon,
                EffectsOnUse = true,
                EffectsOnThrow = true,
                SkillOnUse = new SkillDataOnUse(
                    new AtFeet(),
                    mold.Area,
                    new AttackEffect(Mathf.RoundToInt(material.Power * mold.PowerMagnification * prefix.PowerMagnification), prefix.AdditionalConditions)
                ),
                SkillOnThrow = new SkillDataOnThrow(
                    new SelfArea(),
                    new AttackEffect(Mathf.RoundToInt(material.Power * mold.PowerMagnification * prefix.PowerMagnification), prefix.AdditionalConditions)
                ),
                UsageLimit = Mathf.RoundToInt(mold.UsageLimit * material.UsageLimitMagnification * prefix.UsageLimitMagnification),
            };
        }
    }
}

