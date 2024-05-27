#nullable enable
using Model;
using R3;
using System.Linq;
using Model.Domain.Map;
using Model.Game;
using Utilities;
using VContainer;
using View;
using UnityEngine;

namespace Provider
{
    public class TilemapPresenter
    {
        private SerialDisposable[] _disposables = Enumerable.Range(0, 3).Select(_ => new SerialDisposable()).ToArray();

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

                foreach (var (position, _) in mapLoaded.Tilemap.GetAllTiles())
                {
                    tileMask.SetTileTransparent(position); // HACK: Separating this due to a bug when not separated
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
            });
            // HACK: Here.
            world.PlayerEvents.OnVisibleAreaChanged.Subscribe(visibleAreaChanged =>
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
        }
    }
}