#nullable enable
using System.Linq;
using Model.Map;
using R3;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class TilemapPresenter
    {
        [Inject]
        public TilemapPresenter(TileViewController tileView, TileMaskController tileMask, Tilemap tilemap)
        {
            foreach ((var position, var tileData) in tilemap.GetAllTiles())
                switch (tileData.TileType)
                {
                    case TileCategory.Wall:
                        tileView.SetWall(position);
                        break;
                    case TileCategory.Floor:
                        tileView.SetFloor(position);
                        break;
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