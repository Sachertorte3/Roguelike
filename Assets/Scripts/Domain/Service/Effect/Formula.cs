using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character.Status;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using UnityEngine;

namespace Domain.Service.Effect
{
    internal static class Formula
    {
        public static int Calc(IActorOfEffect actor, ITargetOfEffect target, List<ElementPower> powers,
            bool isCritical = false)
        {
            if (target.Status.IsFlagStat(FlagStatType.Hard) && !isCritical)
                return 1;

            var elementDamages = new List<float>();
            foreach (var elementPower in powers)
            {
                var elementAttackMultiplier = actor.GetElementAttackMultiplier(elementPower.Element);
                var elementResistanceMultiplier = target.GetElementDamageRateMultiplier(elementPower.Element);
                elementDamages.Add(elementPower.Power * elementAttackMultiplier * elementResistanceMultiplier);
            }

            return Mathf.Max(1, Mathf.RoundToInt(elementDamages.Sum() * (isCritical ? 2 : 1)));
        }

        public static int EvaluateDamage(List<ElementPower> powers, bool isCritical = false)
        {
            return Mathf.RoundToInt(powers.Sum(power => power.Power) * (isCritical ? 2 : 1));
        }

        public static int CalcHeal(int power)
        {
            return Mathf.RoundToInt(power);
        }

        public static int EvaluateHeal(int power)
        {
            return Mathf.RoundToInt(power);
        }

        public static int CalcExplosionDamage(float damageRate, ITargetOfEffect target)
        {
            return Mathf.Max(1, Mathf.RoundToInt(target.CurrentHp * damageRate));
        }

        public static int EvaluateExplosionDamage(float damageRate)
        {
            return Mathf.RoundToInt(CommonSenseParameters.PlayerMaxHealth * damageRate);
        }
    }
}