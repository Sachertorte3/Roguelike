#nullable enable
using System.Linq;
using Domain.Model.Map;
using Model.Game;
using R3;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class TilemapPresenter
    {
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public TilemapPresenter(TileViewController tileView, TileMaskController tileMask, World world)
        {
            world.ActiveMap.SubscribeToAllIgnoreNull(mapLoaded =>
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
                            if (mapLoaded.VisibleArea.Contains(position))
                            {
                                tileMask.SetTileVisible(position);
                            }
                            else
                            {
                                tileMask.SetTileTranslucent(position);
                            }
                        }
                        else
                        {
                            tileMask.SetTileTransparent(position);
                        }
                    }

                    _disposables.Add(mapLoaded.Tilemap.OnTileChanged.Subscribe(context =>
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
                    }));
                    // HACK: The following subscription might conflict with the one below if their handling logic diverges in the future.
                    _disposables.Add(mapLoaded.Tilemap.OnTileKnownChanged.Subscribe(context =>
                    {
                        if (context.tile.IsKnown)
                        {
                            tileMask.SetTileVisible(context.position);
                        }
                        else
                        {
                            tileMask.SetTileTransparent(context.position);
                        }
                    }));
                    // HACK: Here.
                    _disposables.Add(mapLoaded.CharacterManager.PlayerEvents.OnVisibleAreaChanged.Subscribe(
                        visibleAreaChanged =>
                        {
                            foreach (var position in visibleAreaChanged.Message.AreaEntered)
                            {
                                tileMask.SetTileVisible(position);
                            }

                            foreach (var position in visibleAreaChanged.Message.AreaExited)
                            {
                                tileMask.SetTileTranslucent(position);
                            }
                        }));
                },
                _ => _disposables.Clear());
        }

        ~TilemapPresenter()
        {
            Dispose();
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}