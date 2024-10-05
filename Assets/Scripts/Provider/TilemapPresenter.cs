#nullable enable
using System;
using System.Linq;
using Domain.Model.Dungeon;
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
        public TilemapPresenter(TileViewController tileView, GrassViewController grassView, World world)
        {
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
                {
                    tileView.Clear();
                    grassView.Clear();

                    var tileSet = map.Type switch
                    {
                        SectionType.Cave => TileSet.Cave,
                        SectionType.Forest => TileSet.Forest,
                        SectionType.Snow => TileSet.Snow,
                        SectionType.Volcano => TileSet.Volcano,
                        SectionType.Dungeon => TileSet.Dungeon,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    foreach (var (position, tileData) in map.TilemapViewer.GetAllTiles())
                    {
                        SetTile(tileView, tileData, position, tileSet, map.ShopRect);
                        if (map.TilemapViewer.GetAllGrasses().Contains(position))
                            grassView.SetGrass(position);
                        SetVisibility(tileView, grassView, position, GetTileVisibility(map, position));
                    }

                    var mapSize = map.TilemapViewer.Rect;
                    for (var x = mapSize.x - 1; x <= mapSize.x + mapSize.width; x++)
                    {
                        tileView.SetUnbreakableWall(new Vector2Int(x, -1), tileSet, TileVisibility.Transparent);
                        tileView.SetUnbreakableWall(new Vector2Int(x, mapSize.y + mapSize.height), tileSet,
                            TileVisibility.Transparent);
                    }

                    for (var y = mapSize.y - 1; y <= mapSize.y + mapSize.height; y++)
                    {
                        tileView.SetUnbreakableWall(new Vector2Int(-1, y), tileSet, TileVisibility.Transparent);
                        tileView.SetUnbreakableWall(new Vector2Int(mapSize.x + mapSize.width, y), tileSet,
                            TileVisibility.Transparent);
                    }

                    _disposables.Add(map.TilemapViewer.OnTilesChanged.Subscribe(context =>
                    {
                        foreach (var (position, tile) in context)
                        {
                            SetTile(tileView, tile, position, tileSet, map.ShopRect);
                        }
                    }));

                    _disposables.Add(map.TilemapViewer.OnGrassesChanged.Subscribe(context =>
                    {
                        foreach (var (position, isGrass) in context)
                        {
                            if (isGrass)
                                grassView.SetGrass(position, GetTileVisibility(map, position));
                            else
                                grassView.RemoveGrass(position);
                        }
                    }));
                    // HACK: The following subscription might conflict with the one below if their handling logic diverges in the future.
                    _disposables.Add(map.TilemapViewer.OnTilesKnownChanged.Subscribe(context =>
                    {
                        foreach (var (position, isKnown) in context)
                        {
                            if (isKnown)
                            {
                                SetVisibility(tileView, grassView, position, TileVisibility.Visible);
                            }
                            else
                            {
                                SetVisibility(tileView, grassView, position, TileVisibility.Transparent);
                            }
                        }
                    }));
                    // HACK: Here.
                    _disposables.Add(map.CharacterManager.PlayerEvents.OnVisibleAreaChanged.Subscribe(
                        visibleAreaChanged =>
                        {
                            var areaEntered =
                                visibleAreaChanged.Message.NewArea.Except(visibleAreaChanged.Message.OldArea);
                            var areaExited =
                                visibleAreaChanged.Message.OldArea.Except(visibleAreaChanged.Message.NewArea);
                            foreach (var position in areaEntered)
                            {
                                SetVisibility(tileView, grassView, position, TileVisibility.Visible);
                            }

                            foreach (var position in areaExited)
                            {
                                SetVisibility(tileView, grassView, position, TileVisibility.Translucent);
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

        public TileVisibility GetTileVisibility(MapManager map, Vector2Int position)
        {
            if (map.TilemapViewer.GetTile(position).MapOr(false, tile => tile.IsKnown))
            {
                if (map.VisibleArea.Contains(position))
                {
                    return TileVisibility.Visible;
                }

                return TileVisibility.Translucent;
            }

            return TileVisibility.Transparent;
        }

        public void SetTile(TileViewController tileView, TileData tileData, Vector2Int position, TileSet type,
            RectInt? shop, TileVisibility? visibility = null)
        {
            switch (tileData.TileType)
            {
                case TileCategory.Floor:
                    if (shop.HasValue && shop.Value.Contains(position))
                        tileView.SetShopFloor(position, type, visibility);
                    else
                        tileView.SetFloor(position, type, visibility);
                    break;
                case TileCategory.Water:
                    tileView.SetWater(position, type, visibility);
                    break;
                case TileCategory.Wall:
                    tileView.SetWall(position, type, visibility);
                    break;
                case TileCategory.UnbreakableWall:
                    tileView.SetUnbreakableWall(position, type, visibility);
                    break;
            }
        }

        public void SetVisibility(TileViewController tileView, GrassViewController grassView, Vector2Int position,
            TileVisibility visibility)
        {
            tileView.SetTileColor(position, visibility.GetColor());
            grassView.SetTileColor(position, visibility.GetColor());
        }
    }
}