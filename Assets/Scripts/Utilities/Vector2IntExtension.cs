#nullable enable
using UnityEngine;

namespace Utilities
{
    public static class Vector2Extension
    {
        public static float ChebyshevDistance(Vector2 a, Vector2 b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }
        public static float ChebyshevDistance(this Vector2 vector)
        {
            return Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        }
    }
}