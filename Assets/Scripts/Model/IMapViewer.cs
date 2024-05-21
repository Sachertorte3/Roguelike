#nullable enable
using Model.Characters;
using Model.Items;
using Model.Map;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    internal interface IMapViewer
    {
        public ITilemapViewer Tilemap { get; }
    }
}