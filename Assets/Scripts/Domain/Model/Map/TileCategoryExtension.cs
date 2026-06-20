using System.ComponentModel;

namespace Domain.Model.Map
{
    // 地形タイプ（TileCategory）そのものの性質を定義する、通行・視界判定の最小単位。
    // ここでの Walkable / Passable / Transparent の違いがマス単位・キャラ単位の判定の土台になる。
    // 違いは Water に表れる: 立てない(Walkable=false) が、通過と光は通す(Passable / Transparent=true)。
    public static class TileCategoryExtension
    {
        /// <summary>立ち止まれる地形か（床のみ true）。水は通れても立てないため false。</summary>
        public static bool IsWalkable(this TileCategory tileCategory)
        {
            return tileCategory switch
            {
                TileCategory.Floor => true,
                TileCategory.Water => false,
                TileCategory.Wall => false,
                TileCategory.UnbreakableWall => false,
                TileCategory.Blank => false,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        /// <summary>通過できる地形か（床・水は true）。立てるかどうかは問わない（Walkable ⊂ Passable）。</summary>
        public static bool IsPassable(this TileCategory tileCategory)
        {
            return tileCategory switch
            {
                TileCategory.Floor => true,
                TileCategory.Water => true,
                TileCategory.Wall => false,
                TileCategory.UnbreakableWall => false,
                TileCategory.Blank => false,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        /// <summary>光を通す地形か（床・水は true）。視界=FOV 計算で遮蔽の有無に使う。</summary>
        public static bool IsTransparent(this TileCategory tileCategory)
        {
            return tileCategory switch
            {
                TileCategory.Floor => true,
                TileCategory.Water => true,
                TileCategory.Wall => false,
                TileCategory.UnbreakableWall => false,
                TileCategory.Blank => false,
                _ => throw new InvalidEnumArgumentException()
            };
        }
    }
}