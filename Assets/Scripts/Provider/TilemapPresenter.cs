#nullable enable
using R3;
using Scripts.Model.Map;
using Scripts.Utilities;
using Scripts.View;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Scripts.Provider
{
    public class TilemapPresenter
    {
        [Inject]
        public TilemapPresenter(TileViewController tileView, TileMaskController tileMask, Tilemap tilemap)
        {
            foreach ((Vector2Int position, TileData tileData) in tilemap.GetAllTiles())
            {
                switch (tileData.TileType)
                {
                    case TileCategory.Wall:
                        tileView.SetWall(position);
                        break;
                    case TileCategory.Floor:
                        tileView.SetFloor(position);
                        break;
                }
            }

            tileMask.SetTilesTransparent(tilemap.Rect.RectRange().ToHashSet());

            tilemap.OnChangeTile.Subscribe(context =>
            {
                switch (context.tile.TileType)
                {
                    case TileCategory.Wall:
                        tileView.SetWall(context.position);
                        break;
                    case TileCategory.Floor:
                        tileView.SetFloor(context.position);
                        break;
                }
                tileMask.ResetMask(context.position);
            });
        }
    }
}