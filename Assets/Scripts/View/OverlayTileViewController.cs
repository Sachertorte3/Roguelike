using UnityEngine;
using UnityEngine.Tilemaps;

namespace View
{
    public sealed class OverlayTileViewController : MonoBehaviour
    {
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private TileBase _grass;
        [SerializeField] private TileBase _ice;

        public void Clear()
        {
            _tilemap.ClearAllTiles();
        }

        public void SetGrass(Vector2Int position, TileVisibility? visibility = null)
        {
            SetTile(position, _grass, visibility);
        }

        public void SetIce(Vector2Int position, TileVisibility? visibility = null)
        {
            SetTile(position, _ice, visibility);
        }

        public void RemoveTile(Vector2Int position)
        {
            SetTile(position, null);
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

        public void SetTileVisibility(Vector2Int position, TileVisibility visibility)
        {
            SetTileColor(position, visibility.GetColor());
        }

        private void SetTileColor(Vector2Int position, Color color)
        {
            var vector3 = new Vector3Int(position.x, position.y, 0);
            _tilemap.SetTileFlags(vector3, TileFlags.None);
            _tilemap.SetColor(vector3, color);
        }
    }
}