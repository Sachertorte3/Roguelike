using UnityEngine;
using UnityEngine.Tilemaps;

namespace View
{
    public sealed class TileViewController : MonoBehaviour
    {
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private Tiles _tiles;

        public void Clear()
        {
            _tilemap.ClearAllTiles();
        }

        public void SetFloor(Vector2Int position, TileVisibility? visibility = null)
        {
            SetTile(position, _tiles.Floor, visibility);
        }

        public void SetWater(Vector2Int position, TileVisibility? visibility = null)
        {
            SetTile(position, _tiles.Water, visibility);
        }

        public void SetWall(Vector2Int position, TileVisibility? visibility = null)
        {
            SetTile(position, _tiles.Wall, visibility);
        }

        public void SetUnbreakableWall(Vector2Int position, TileVisibility? visibility = null)
        {
            SetTile(position, _tiles.Wall, visibility);
        }

        public void SetShopFloor(Vector2Int position, TileVisibility? visibility = null)
        {
            SetTile(position, _tiles.ShopFloor, visibility);
        }

        private void SetTile(Vector2Int position, TileBase tile, TileVisibility? visibility = null)
        {
            var color = visibility?.GetColor() ?? GetTileColor(position);
            _tilemap.SetTile(new Vector3Int(position.x, position.y, 0), tile);
            SetTileColor(position, color);
        }
        
        public Color GetTileColor(Vector2Int position)
        {
            var color = _tilemap.GetColor(new Vector3Int(position.x, position.y, 0));
            return color;
        }

        public void SetTileColor(Vector2Int position, Color color)
        {
            var vector3 = new Vector3Int(position.x, position.y, 0);
            _tilemap.SetTileFlags(vector3, TileFlags.None);
            _tilemap.SetColor(vector3, color);
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