using System.Collections.Generic;
using Codice.CM.Common;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Service.Effect;
using UnityEngine;

namespace Domain.Service.Items
{
    public static class WeaponFactory
    {
        public static ItemData Create(MaterialData material, WeaponMold mold)
        {
            return new ItemData(
                material.Name + mold.Name,
                mold.Icon,
                false,
                Rarity.Common,
                new SkillDataOnUse(
                    mold.Position,
                    mold.Area,
                    new AttackEffect(new List<ElementPower> {
                            new(Element.Physical, Mathf.RoundToInt(material.Power * mold.PowerMagnification))
                        },
                        0,
                        new List<AdditionalConditionData>(),
                        0
                    ),
                    CommonSenseParameters.SkillOnUseProbabilityOfSuccess
                ),
                new SkillDataOnThrow(
                    new SelfArea(),
                    new AttackEffect(new List<ElementPower> {
                            new(Element.Physical, Mathf.RoundToInt(material.Power * mold.PowerMagnification))
                        },
                        0,
                        new List<AdditionalConditionData>(),
                        0
                    ),
                    CommonSenseParameters.SkillOnThrowProbabilityOfSuccess
                ),
                true,
                false,
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
                false,
                prefix.Rarity,
                new SkillDataOnUse(
                    mold.Position,
                    mold.Area,
                    new AttackEffect(
                        new List<ElementPower> {
                            new(Element.Physical, Mathf.RoundToInt(material.Power * mold.PowerMagnification * prefix.PowerMagnification))
                        },
                        0,
                        prefix.AdditionalConditions, 0),
                    CommonSenseParameters.SkillOnUseProbabilityOfSuccess
                ),
                new SkillDataOnThrow(
                    new SelfArea(),
                    new AttackEffect(
                        new List<ElementPower> {
                            new(Element.Physical, Mathf.RoundToInt(material.Power * mold.PowerMagnification * prefix.PowerMagnification))
                        },
                        0,
                        prefix.AdditionalConditions, 0),
                    CommonSenseParameters.SkillOnThrowProbabilityOfSuccess
                ),
                true,
                false,
                false,
                Mathf.RoundToInt(mold.UsageLimit * material.UsageLimitMagnification * prefix.UsageLimitMagnification),
                new List<IConditionData>()
            );
        }
    }
}