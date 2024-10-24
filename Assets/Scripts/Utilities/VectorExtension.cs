#nullable enable
using UnityEngine;

namespace Utilities
{
    public static class VectorExtension
    {
        public static float ChebyshevDistance(Vector2 a, Vector2 b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }

        public static float ChebyshevDistance(this Vector2 vector)
        {
            return Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        }

        public static bool IsInteger(this Vector3 vector)
        {
            return vector.Approximately(vector.RoundToInt());
        }

        public static Vector3Int RoundToInt(this Vector3 vector)
        {
            return new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
        }

        public static bool Approximately(this Vector3 a, Vector3 b)
        {
            return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
        }
    }
}