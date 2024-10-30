using System;
using UnityEngine;

namespace View
{
    public static class TileVisibilityExtension
    {
        public static Color GetColor(this TileVisibility visibility)
        {
            return visibility switch
            {
                TileVisibility.Visible => Color.white,
                TileVisibility.Translucent => new Color(0.5f, 0.5f, 0.5f, 1f),
                TileVisibility.Transparent => new Color(0f, 0f, 0f, 0f),
                _ => throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null)
            };
        }
    }
}