using System;

namespace Domain.Model.Map
{
    public enum TileCategory
    {
        Floor,
        Water,
        Wall,
        UnbreakableWall,
        Blank
    }

    public enum OverlayTileCategory
    {
        Grass,
        FloatingIce
    }

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