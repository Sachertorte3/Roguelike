using System.Collections.Generic;
using Domain.Model;
using Domain.Model.Area;
using Effect.Position;
using Domain.Service.Effect;
using UnityEngine;

namespace Domain.Service
{
    public static class WeaponFactory
    {
        public static ItemData Create(MaterialData material, WeaponMold mold)
        {
            return new ItemData(
                material.Name + mold.Name,
                mold.Icon,
                Rarity.Common,
                true,
                true,
                new SkillDataOnUse(
                    new AtFeet(),
                    mold.Area,
                    new AttackEffect(Mathf.RoundToInt(material.Power * mold.PowerMagnification),
                        new List<AdditionalConditionData>())
                ),
                new SkillDataOnThrow(
                    new SelfArea(),
                    new AttackEffect(Mathf.RoundToInt(material.Power * mold.PowerMagnification),
                        new List<AdditionalConditionData>())
                ),
                Mathf.RoundToInt(mold.UsageLimit * material.UsageLimitMagnification)
            );
        }

        public static ItemData Create(WeaponPrefix prefix, MaterialData material, WeaponMold mold)
        {
            return new ItemData(
                prefix.Name + material.Name + mold.Name,
                mold.Icon,
                prefix.Rarity,
                true,
                true,
                new SkillDataOnUse(
                    new AtFeet(),
                    mold.Area,
                    new AttackEffect(
                        Mathf.RoundToInt(material.Power * mold.PowerMagnification * prefix.PowerMagnification),
                        prefix.AdditionalConditions)
                ),
                new SkillDataOnThrow(
                    new SelfArea(),
                    new AttackEffect(
                        Mathf.RoundToInt(material.Power * mold.PowerMagnification * prefix.PowerMagnification),
                        prefix.AdditionalConditions)
                ),
                Mathf.RoundToInt(mold.UsageLimit * material.UsageLimitMagnification * prefix.UsageLimitMagnification)
            );
        }
    }
}