using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Effect;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    internal static class Formula
    {
        public static int Calc(IActorOfEffect actor, ITargetOfEffect target, List<ElementPower> powers, bool isCritical=false)
        {
            var elementDamages = new List<float>();
            foreach (var elementPower in powers)
            {
                var elementAttackMultiplier = actor.GetElementAttackMultiplier(elementPower.Element);
                var elementResistanceMultiplier = target.GetElementDamageRateMultiplier(elementPower.Element);
                elementDamages.Add(elementPower.Power * elementAttackMultiplier * elementResistanceMultiplier);
            }
            return Mathf.RoundToInt(elementDamages.Sum() * (isCritical ? 2 : 1));
        }
        public static int CalcHeal(int power)
        {
            var baseHeal = power;
            return Mathf.RoundToInt(baseHeal);
        }
    }
}