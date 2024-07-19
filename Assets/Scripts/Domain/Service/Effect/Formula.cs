using Domain.Model.Effect;
using UnityEngine;

namespace Domain.Service.Effect
{
    internal static class Formula
    {
        public static int Calc(IActorOfEffect actor, int power)
        {
            return Mathf.RoundToInt(power * actor.AttackMultiplier);
        }
    }
}