using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Item;
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
                new SkillDataOnUse(
                    mold.Position,
                    mold.Area,
                    new AttackEffect(Mathf.RoundToInt(material.Power * mold.PowerMagnification),
                        new List<AdditionalConditionData>(), 0)
                ),
                new SkillDataOnThrow(
                    new SelfArea(),
                    new AttackEffect(Mathf.RoundToInt(material.Power * mold.PowerMagnification),
                        new List<AdditionalConditionData>(), 0)
                ),
                false,
                Mathf.RoundToInt(mold.UsageLimit * material.UsageLimitMagnification),
                new List<IConditionData>()
            );
        }

        public static ItemData Create(WeaponPrefix prefix, MaterialData material, WeaponMold mold)
        {
            return new ItemData(
                prefix.Name + material.Name + mold.Name,
                mold.Icon,
                prefix.Rarity,
                new SkillDataOnUse(
                    mold.Position,
                    mold.Area,
                    new AttackEffect(
                        Mathf.RoundToInt(material.Power * mold.PowerMagnification * prefix.PowerMagnification),
                        prefix.AdditionalConditions, 0)
                ),
                new SkillDataOnThrow(
                    new SelfArea(),
                    new AttackEffect(
                        Mathf.RoundToInt(material.Power * mold.PowerMagnification * prefix.PowerMagnification),
                        prefix.AdditionalConditions, 0)
                ),
                false,
                Mathf.RoundToInt(mold.UsageLimit * material.UsageLimitMagnification * prefix.UsageLimitMagnification),
                new List<IConditionData>()
            );
        }
    }
}