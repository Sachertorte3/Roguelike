using System;

namespace Domain.Model.Map
{
    public static class OverlayTileCategoryExtensions
    {
        public static TileCategory GetPlaceableTileCategory(this OverlayTileCategory category)
        {
            return category switch
            {
                OverlayTileCategory.Grass => TileCategory.Floor,
                OverlayTileCategory.FloatingIce => TileCategory.Water,
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };
        }
    }
}