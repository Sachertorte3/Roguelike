using RandomDungeonWithBluePrint;
using Scripts.Model.Characters;
using Scripts.Model.Map;
using Scripts.Model;
using Scripts.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using UI;
using R3;
using Scripts.Utilities;

namespace Assets.Scripts.Provider
{
    internal class WorldPresenter
    {
        [Inject]
        public WorldPresenter(CharacterManager characterManager, TileViewController tileView, TileMaskController tileMask, FieldBluePrint bluePrint)
        {
            Tilemap map = CreateTilemap(bluePrint, tileView, tileMask);
            SetTilemapView(tileView, tileMask, map);
            CreateWorld(map, characterManager);
        }
        private Tilemap CreateTilemap(FieldBluePrint bluePrint, TileViewController tileView, TileMaskController tileMask)
        {
            Tilemap map = new Tilemap(bluePrint);
            map.OnChangeTile.Subscribe(context =>
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
            return map;
        }
        private void SetTilemapView(TileViewController tileView, TileMaskController tileMask, Tilemap map)
        {
            foreach ((Vector2Int position, TileData tileData) in map.GetAllTiles())
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
            tileMask.SetTilesTransparent(map.Rect.RectRange().ToHashSet());
        }
        private void CreateWorld(Tilemap map, CharacterManager characterManager)
        {
            World world = new World(map, characterManager);
            Globals.World = world;
        }
    }
}
