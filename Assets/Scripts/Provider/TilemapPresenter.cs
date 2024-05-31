#nullable enable
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
        private SerialDisposable[] _disposables = EnumerableExtension.CreateArrayWithNewInstances<SerialDisposable>(4).ToArray();

        [Inject]
        public TilemapPresenter(TileViewController tileView, TileMaskController tileMask, World world)
        {
            _disposables[0].Disposable = world.ActiveMap.SubscribeToAll(mapLoaded =>
            {
                tileView.Clear();

                foreach (var (position, tileData) in mapLoaded.Tilemap.GetAllTiles())
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

                foreach (var (position, tileData) in mapLoaded.Tilemap.GetAllTiles())
                {
                    // HACK: Separating this due to a bug when not separated
                    if (tileData.IsKnown)
                    {
                        tileMask.SetTileVisible(position);
                    }
                    else
                    {
                        tileMask.SetTileTransparent(position);
                    }
                }

                _disposables[1].Disposable = mapLoaded.Tilemap.OnTileChanged.Subscribe(context =>
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
                });
                // HACK: The following subscription might conflict with the one below if their handling logic diverges in the future.
                _disposables[2].Disposable = mapLoaded.Tilemap.OnTileKnownChanged.Subscribe(context =>
                {
                    if (context.tile.IsKnown)
                    {
                        tileMask.SetTileVisible(context.position);
                    }
                    else
                    {
                        tileMask.SetTileTransparent(context.position);
                    }
                });
                // HACK: Here.
                _disposables[3].Disposable = mapLoaded.CharacterManager.PlayerEvents.OnVisibleAreaChanged.Subscribe(visibleAreaChanged =>
                {
                    foreach (var position in visibleAreaChanged.Message.AreaEntered)
                    {
                        tileMask.SetTileVisible(position);
                    }
                    foreach (var position in visibleAreaChanged.Message.AreaExited)
                    {
                        tileMask.SetTileTranslucent(position);
                    }
                });
            });
        }
    }
}