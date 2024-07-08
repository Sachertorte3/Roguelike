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

        public void SetWall(Vector2Int position)
        {
            SetTile(position, _tiles.Wall);
        }

        public void SetFloor(Vector2Int position)
        {
            SetTile(position, _tiles.Floor);
        }

        public void SetShopFloor(Vector2Int position)
        {
            SetTile(position, _tiles.ShopFloor);
        }

        private void SetTile(Vector2Int position, TileBase tile)
        {
            _tilemap.SetTile(new Vector3Int(position.x, position.y, 0), tile);
        }
    }
}