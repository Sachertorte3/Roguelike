using UnityEngine;

namespace Utilities
{
    public record Angle
    {
        public Angle(float value)
        {
            Value = ((value % 360) + 360) % 360;
        }

        public Angle(Vector2 vector) : this(Vector2.SignedAngle(Vector2.right, vector))
        {
        }

        public float Value { get; init; }

        public Angle RotateClockwise(float value)
        {
            return new Angle(Value - value);
        }

        public Angle RotateAntiClockwise(float value)
        {
            return new Angle(Value + value);
        }

        public static Angle operator +(Angle left, Angle right)
        {
            return new Angle(left.Value + right.Value);
        }

        public static Angle operator -(Angle left, Angle right)
        {
            return new Angle(left.Value - right.Value);
        }
    }
}