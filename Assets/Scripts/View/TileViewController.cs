using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace View
{
    public sealed class TileViewController : MonoBehaviour
    {
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private Tiles _caveTiles;
        [SerializeField] private Tiles _forestTiles;
        [SerializeField] private Tiles _snowTiles;
        [SerializeField] private Tiles _volcanoTiles;
        [SerializeField] private Tiles _dungeonTiles;

        private Tiles GetTiles(TileSet type)
        {
            return type switch
            {
                TileSet.Cave => _caveTiles,
                TileSet.Forest => _forestTiles,
                TileSet.Snow => _snowTiles,
                TileSet.Volcano => _volcanoTiles,
                TileSet.Dungeon => _dungeonTiles,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public void Clear()
        {
            _tilemap.ClearAllTiles();
        }

        public void SetFloor(Vector2Int position, TileSet type, TileVisibility? visibility = null)
        {
            SetTile(position, GetTiles(type).Floor, visibility);
        }

        public void SetWater(Vector2Int position, TileSet type, TileVisibility? visibility = null)
        {
            SetTile(position, GetTiles(type).Water, visibility, GetTiles(type).Floor);
        }

        public void SetWall(Vector2Int position, TileSet type, TileVisibility? visibility = null)
        {
            SetTile(position, GetTiles(type).Wall, visibility, GetTiles(type).Floor);
        }

        public void SetUnbreakableWall(Vector2Int position, TileSet type, TileVisibility? visibility = null)
        {
            SetTile(position, GetTiles(type).Wall, visibility, GetTiles(type).Floor);
        }

        public void SetShopFloor(Vector2Int position, TileSet type, TileVisibility? visibility = null)
        {
            SetTile(position, GetTiles(type).ShopFloor, visibility);
        }

        private void SetTile(Vector2Int position, TileBase tile, TileVisibility? visibility = null,
            TileBase underTile = null)
        {
            var color = visibility?.GetColor() ?? GetTileColor(position);
            _tilemap.SetTile(new Vector3Int(position.x, position.y, 0), tile);
            _tilemap.SetTile(new Vector3Int(position.x, position.y, -1), underTile);
            SetTileColor(position, color);
        }

        public Color GetTileColor(Vector2Int position)
        {
            var color = _tilemap.GetColor(new Vector3Int(position.x, position.y, 0));
            return color;
        }

        public void SetTileColor(Vector2Int position, Color color)
        {
            _tilemap.SetTileFlags(new Vector3Int(position.x, position.y, 0), TileFlags.None);
            _tilemap.SetColor(new Vector3Int(position.x, position.y, 0), color);
            _tilemap.SetTileFlags(new Vector3Int(position.x, position.y, -1), TileFlags.None);
            _tilemap.SetColor(new Vector3Int(position.x, position.y, -1), color);
        }

        public void SetTileTransparent(Vector2Int position)
        {
            SetTileColor(position, TileVisibility.Transparent.GetColor());
        }

        public void SetTileTranslucent(Vector2Int position)
        {
            SetTileColor(position, TileVisibility.Translucent.GetColor());
        }

        public void SetTileVisible(Vector2Int position)
        {
            SetTileColor(position, TileVisibility.Visible.GetColor());
        }
    }
}