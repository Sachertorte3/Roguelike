using UnityEngine;
using UnityEngine.Tilemaps;

namespace View
{
    public class TileMaskController : MonoBehaviour
    {
        [SerializeField] private Tilemap _tilemap;

        public void SetTileColor(Vector2Int position, Color color)
        {
            _tilemap.SetTileFlags(new Vector3Int(position.x, position.y, 0), TileFlags.None);
            _tilemap.SetColor(new Vector3Int(position.x, position.y, 0), color);
        }

        public void SetTileTransparent(Vector2Int position)
        {
            SetTileColor(position, Color.clear);
        }

        public void SetTileTranslucent(Vector2Int position)
        {
            SetTileColor(position, new Color(1f, 1f, 1f, 0.5f));
        }

        public void SetTileVisible(Vector2Int position)
        {
            SetTileColor(position, Color.white);
        }
    }
}