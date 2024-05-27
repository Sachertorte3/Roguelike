using Data.Effect;

namespace Model.Domain.Effect
{
    internal static class Formula
    {
        public static int Calc(IActorOfEffect actor, int power)
        {
            return power;
        }
    }
}