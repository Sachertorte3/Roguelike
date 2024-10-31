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
        public TilemapPresenter(TileViewController tileView, OverlayTileViewController overlayTileView, MinimapController minimapController, World world)
        {
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
                {
                    tileView.Clear();
                    overlayTileView.Clear();

                    var tileSet = map.Type switch
                    {
                        SectionType.Cave => TileSet.Cave,
                        SectionType.Forest => TileSet.Forest,
                        SectionType.Snow => TileSet.Snow,
                        SectionType.Volcano => TileSet.Volcano,
                        SectionType.Desert => TileSet.Desert,
                        SectionType.Dungeon => TileSet.Dungeon,
                        SectionType.Void => TileSet.Void,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    foreach (var (position, tileData) in map.TilemapViewer.GetAllTiles())
                    {
                        SetTile(tileView, minimapController, tileData, position, tileSet, map.ShopRect);
                        if (map.TilemapViewer.GetAllGrasses().Contains(position))
                            overlayTileView.SetGrass(position);
                        if (map.TilemapViewer.GetAllIces().Contains(position))
                            overlayTileView.SetIce(position);
                        SetVisibility(tileView, overlayTileView, minimapController, position, GetTileVisibility(map, position));
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
                            SetTile(tileView, minimapController, tile, position, tileSet, map.ShopRect);
                        }
                    }));

                    _disposables.Add(map.TilemapViewer.OnOverlayTilesChanged.Subscribe(context =>
                    {
                        foreach (var (position, category) in context)
                        {
                            switch (category)
                            {
                                case OverlayTileCategory.Grass:
                                    overlayTileView.SetGrass(position, GetTileVisibility(map, position));
                                    break;
                                case OverlayTileCategory.FloatingIce:
                                    overlayTileView.SetIce(position, GetTileVisibility(map, position));
                                    break;
                                case null:
                                    overlayTileView.RemoveTile(position);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
                            }
                        }
                    }));
                    // HACK: The following subscription might conflict with the one below if their handling logic diverges in the future.
                    _disposables.Add(map.TilemapViewer.OnTilesKnownChanged.Subscribe(context =>
                    {
                        foreach (var (position, isKnown) in context)
                        {
                            if (isKnown)
                            {
                                SetVisibility(tileView, overlayTileView, minimapController, position, TileVisibility.Visible);
                            }
                            else
                            {
                                SetVisibility(tileView, overlayTileView, minimapController, position, TileVisibility.Transparent);
                            }
                        }
                    }));
                    // HACK: Here.
                    var previousVisibleArea = map.VisibleArea;
                    _disposables.Add(map.CharacterManager.PlayerEvents.OnVisibleAreaChanged
                        .Select(x => x.Character.VisionRange.VisibleArea)
                        .Subscribe(visibleAreaChanged =>
                        {
                            var areaEntered = visibleAreaChanged.Except(previousVisibleArea);
                            var areaExited = previousVisibleArea.Except(visibleAreaChanged);
                            previousVisibleArea = visibleAreaChanged;
                            foreach (var position in areaEntered)
                            {
                                SetVisibility(tileView, overlayTileView, minimapController, position, TileVisibility.Visible);
                            }

                            foreach (var position in areaExited)
                            {
                                SetVisibility(tileView, overlayTileView, minimapController, position, TileVisibility.Translucent);
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
                if (map.Player.IsVisible(position))
                {
                    return TileVisibility.Visible;
                }

                return TileVisibility.Translucent;
            }

            return TileVisibility.Transparent;
        }

        public void SetTile(TileViewController tileView, MinimapController minimapController, TileData tileData, Vector2Int position, TileSet type,
            RectInt? shop, TileVisibility? visibility = null)
        {
            switch (tileData.TileType)
            {
                case TileCategory.Floor:
                    if (shop.HasValue && shop.Value.Contains(position))
                    {
                        tileView.SetShopFloor(position, type, visibility);
                        minimapController.SetShopFloor(position, visibility);
                    }
                    else
                    {
                        tileView.SetFloor(position, type, visibility);
                        minimapController.SetFloor(position, visibility);
                    }
                    break;
                case TileCategory.Water:
                    tileView.SetWater(position, type, visibility);
                    minimapController.SetWater(position, visibility);
                    break;
                case TileCategory.Wall:
                    tileView.SetWall(position, type, visibility);
                    minimapController.SetWall(position, visibility);
                    break;
                case TileCategory.UnbreakableWall:
                    tileView.SetUnbreakableWall(position, type, visibility);
                    minimapController.SetUnbreakableWall(position, visibility);
                    break;
            }
        }

        public void SetVisibility(TileViewController tileView, OverlayTileViewController overlayTileView, MinimapController minimapController, Vector2Int position,
            TileVisibility visibility)
        {
            tileView.SetTileVisibility(position, visibility);
            overlayTileView.SetTileVisibility(position, visibility);
            minimapController.SetTileVisibility(position, visibility);
        }
    }
}