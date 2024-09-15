#nullable enable
using System.Linq;
using Domain.Model.Map;
using Game;
using R3;
using UnityEngine;
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

                    foreach (var (position, tileData) in mapLoaded.TilemapViewer.GetAllTiles())
                    {
                        SetTile(tileView, tileData, position, mapLoaded.ShopRect);
                    }

                    var mapSize = mapLoaded.TilemapViewer.Rect;
                    for (int x = mapSize.x - 1; x <= mapSize.x + mapSize.width; x++)
                    {
                        tileView.SetUnbreakableWall(new Vector2Int(x, -1));
                        tileView.SetUnbreakableWall(new Vector2Int(x, mapSize.y + mapSize.height));
                    }
                    for (int y = mapSize.y - 1; y <= mapSize.y + mapSize.height; y++)
                    {
                        tileView.SetUnbreakableWall(new Vector2Int(-1, y));
                        tileView.SetUnbreakableWall(new Vector2Int(mapSize.x + mapSize.width, y));
                    }
                    for (int x = mapSize.x - 1; x <= mapSize.x + mapSize.width; x++)
                    {
                        tileMask.SetTileTransparent(new Vector2Int(x, -1));
                        tileMask.SetTileTransparent(new Vector2Int(x, mapSize.y + mapSize.height));
                    }
                    for (int y = mapSize.y - 1; y <= mapSize.y + mapSize.height; y++)
                    {
                        tileMask.SetTileTransparent(new Vector2Int(-1, y));
                        tileMask.SetTileTransparent(new Vector2Int(mapSize.x + mapSize.width, y));
                    }

                    foreach (var (position, tileData) in mapLoaded.TilemapViewer.GetAllTiles())
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

                    _disposables.Add(mapLoaded.TilemapViewer.OnTilesChanged.Subscribe(context =>
                    {
                        foreach (var (position, tile) in context)
                        {
                            SetTile(tileView, tile, position, mapLoaded.ShopRect);
                        }
                    }));
                    // HACK: The following subscription might conflict with the one below if their handling logic diverges in the future.
                    _disposables.Add(mapLoaded.TilemapViewer.OnTilesKnownChanged.Subscribe(context =>
                    {
                        foreach (var (position, tile) in context)
                        {
                            if (tile.IsKnown)
                            {
                                tileMask.SetTileVisible(position);
                            }
                            else
                            {
                                tileMask.SetTileTransparent(position);
                            }
                        }
                    }));
                    // HACK: Here.
                    _disposables.Add(mapLoaded.CharacterManager.PlayerEvents.OnVisibleAreaChanged.Subscribe(
                        visibleAreaChanged =>
                        {
                            var areaEntered = visibleAreaChanged.Message.NewArea.Except(visibleAreaChanged.Message.OldArea);
                            var areaExited = visibleAreaChanged.Message.OldArea.Except(visibleAreaChanged.Message.NewArea);
                            foreach (var position in areaEntered)
                            {
                                tileMask.SetTileVisible(position);
                            }

                            foreach (var position in areaExited)
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

        public void SetTile(TileViewController tileView, TileData tileData, Vector2Int position, RectInt? shop)
        {
            switch (tileData.TileType)
            {
                case TileCategory.Wall:
                    tileView.SetWall(position);
                    break;
                case TileCategory.UnbreakableWall:
                    tileView.SetUnbreakableWall(position);
                    break;
                case TileCategory.Floor:
                    if (shop.HasValue && shop.Value.Contains(position))
                        tileView.SetShopFloor(position);
                    else
                        tileView.SetFloor(position);
                    break;
            }
        }
    }
}