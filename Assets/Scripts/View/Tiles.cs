using System;
using UnityEngine.Tilemaps;

namespace View
{
    [Serializable]
    internal struct Tiles
    {
        public TileBase Wall;
        public TileBase Floor;
        public TileBase ShopFloor;
    }
}