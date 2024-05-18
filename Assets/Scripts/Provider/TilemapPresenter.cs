#nullable enable
using System.Linq;
using Model;
using Model.Map;
using R3;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class TilemapPresenter
    {
        private SerialDisposable _disposable = new();
        [Inject]
        public TilemapPresenter(TileViewController tileView, TileMaskController tileMask, World world)
        {
            world.OnMapLoaded.Subscribe(mapLoaded =>
            {
                tileView.Clear();

                SetTilemap(tileView, tileMask, mapLoaded.Tilemap);
            });
            SetTilemap(tileView, tileMask, world.ActiveMap.Tilemap);
        }
        public void SetTilemap(TileViewController tileView, TileMaskController tileMask, ITilemapViewer tilemap)
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

            _disposable.Disposable = tilemap.OnChangeTile.Subscribe(context =>
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