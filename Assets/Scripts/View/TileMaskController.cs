using Scripts.Utilities;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scripts.View
{
    public class TileMaskController : MonoBehaviour
    {
        [SerializeField] private Tilemap _tilemap;
        public void SetMask(Vector2Int position)
        {
            _tilemap.SetTileFlags(new Vector3Int(position.x, position.y, 0), TileFlags.None);
            _tilemap.SetColor(new Vector3Int(position.x, position.y, 0), Color.clear);
        }
        public void RemoveMask(Vector2Int position)
        {
            _tilemap.SetTileFlags(new Vector3Int(position.x, position.y, 0), TileFlags.None);
            _tilemap.SetColor(new Vector3Int(position.x, position.y, 0), Color.white);
        }
    }
}
