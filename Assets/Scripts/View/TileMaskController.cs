using Scripts.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scripts.View
{
    public class TileMaskController : MonoBehaviour
    {
        [SerializeField] private Tilemap _tilemap;
        public void SetTileColor(Vector2Int position, Color color)
        {
            _tilemap.SetTileFlags(new Vector3Int(position.x, position.y, 0), TileFlags.None);
            _tilemap.SetColor(new Vector3Int(position.x, position.y, 0), color);
        }
        public void SetTilesTransparent(IEnumerable<Vector2Int> positions)
        {
            foreach (Vector2Int position in positions)
            {
                SetTileColor(position, Color.clear);
            }
        }
        public void SetTilesTranslucent(IEnumerable<Vector2Int> positions)
        {
            foreach (Vector2Int position in positions)
            {
                SetTileColor(position, new Color(1f, 1f, 1f, 0.5f));
            }
        }
        public void SetTilesVisible(IEnumerable<Vector2Int> positions)
        {
            foreach (Vector2Int position in positions)
            {
                SetTileColor(position, Color.white);
            }
        }
        public void ResetMask(Vector2Int position)
        {
            SetTilesTransparent(new RectInt(position - new Vector2Int(1, 1), new Vector2Int(3, 3)).RectRange());
        }
    }
}
