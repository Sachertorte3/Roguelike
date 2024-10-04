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
                TileVisibility.Translucent => new Color(1f, 1f, 1f, 0.5f),
                TileVisibility.Transparent => Color.clear,
                _ => throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null)
            };
        }
    }
}