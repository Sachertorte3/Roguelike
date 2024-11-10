using System;

namespace Domain.Model.Character.Status
{
    public static class MoveSpeedExtensions
    {
        public static float ToWaitTime(this MoveSpeed moveSpeed)
        {
            return moveSpeed switch
            {
                MoveSpeed.Quarter => 4,
                MoveSpeed.Half => 2,
                MoveSpeed.Normal => 1,
                MoveSpeed.Double => 0.5f,
                MoveSpeed.Quadruple => 0.25f,
                _ => throw new ArgumentException("Invalid MoveSpeed")
            };
        }
    }
}