#nullable enable
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utilities
{
    public static class RectIntExtension
    {
        public static RectInt GetRandomInnerRect(this RectInt rect, Vector2Int size)
        {
            var max = rect.size - size;

            if (max.x < 0 || max.y < 0)
            {
                throw new ArgumentException("The specified size is larger than rect.");
            }

            var random = new Vector2Int(
                Random.Range(0, max.x + 1),
                Random.Range(0, max.y + 1)
            );

            return new RectInt(rect.min + random, size);
        }

        public static RectInt GetCenteredInnerRect(this RectInt rect, Vector2Int size)
        {
            var max = rect.size - size;

            if (max.x < 0 || max.y < 0)
            {
                throw new ArgumentException("The specified size is larger than rect.");
            }

            var roundedOffset = new Vector2Int(
                Random.value < 0.5f ? Mathf.FloorToInt(max.x / 2f) : Mathf.CeilToInt(max.x / 2f),
                Random.value < 0.5f ? Mathf.FloorToInt(max.y / 2f) : Mathf.CeilToInt(max.y / 2f)
            );

            return new RectInt(rect.min + roundedOffset, size);
        }
    }
}