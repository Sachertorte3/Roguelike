using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scripts.View
{
    public class TileMaskController : MonoBehaviour
    {
        [SerializeField] private Tilemap _tilemap;
        private HashSet<Vector2Int> _lastVisibleArea = new HashSet<Vector2Int>();
        public void SetTileColor(Vector2Int position, Color color)
        {
            _tilemap.SetTileFlags(new Vector3Int(position.x, position.y, 0), TileFlags.None);
            _tilemap.SetColor(new Vector3Int(position.x, position.y, 0), color);
        }
        public void SetTilesTransparent(HashSet<Vector2Int> positions)
        {
            foreach (Vector2Int position in positions)
            {
                SetTileColor(position, Color.clear);
            }
        }
        public void SetTilesTranslucent(HashSet<Vector2Int> positions)
        {
            foreach (Vector2Int position in positions)
            {
                SetTileColor(position, new Color(1f, 1f, 1f, 0.5f));
            }
        }
        public void SetTilesVisible(HashSet<Vector2Int> positions)
        {
            foreach (Vector2Int position in positions)
            {
                SetTileColor(position, Color.white);
            }
        }
        public void Visible(HashSet<Vector2Int> positions)
        {
            SetTilesTranslucent(_lastVisibleArea);
            SetTilesVisible(positions);
            _lastVisibleArea = positions;
        }
    }
}
