using Domain.Model.Effect;

namespace Domain.Service.Effect
{
    internal static class Formula
    {
        public static int Calc(IActorOfEffect actor, int power)
        {
            return power;
        }
    }
}