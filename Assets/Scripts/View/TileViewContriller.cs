using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scripts.View
{
    public sealed class TileViewContriller: MonoBehaviour
    {
        [SerializeField] Tilemap _tilemap;
        [SerializeField] Tiles _tiles;
        public void SetWall(Vector2Int position) => SetTile(position, _tiles.Wall, true);
        public void SetFloor(Vector2Int position) => SetTile(position, _tiles.Floor, true);
        private void SetTile(Vector2Int position, TileBase tile, bool isWall=false)
        {
            _tilemap.SetTile(new Vector3Int(position.x, position.y, isWall ? 0 : -1), tile);
        }
    }
    [Serializable]
    internal struct Tiles
    {
        public TileBase Wall;
        public TileBase Floor;
    }
}
