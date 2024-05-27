#nullable enable
using Model;
using R3;
using System.Linq;
using Model.Domain.Map;
using Model.Game;
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
            _disposable.Disposable = world.ActiveMap.SubscribeToAll(mapLoaded =>
            {
                tileView.Clear();
                foreach (var (position, tileData) in mapLoaded.Tilemap.GetAllTiles())
                    switch (tileData.TileType)
                    {
                        case TileCategory.Wall:
                            tileView.SetWall(position);
                            break;
                        case TileCategory.Floor:
                            tileView.SetFloor(position);
                            break;
                    }

                tileMask.SetTilesTransparent(mapLoaded.Tilemap.Rect.RectRange().ToHashSet());

                _disposable.Disposable = mapLoaded.Tilemap.OnChangeTile.Subscribe(context =>
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
            });
        }
    }
}