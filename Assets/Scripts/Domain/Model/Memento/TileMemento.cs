#nullable enable
using System;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class TileMemento
    {
        [field: SerializeField] public TileCategory TileType { get; private set; }
        [field: SerializeField] public bool IsKnown { get; private set; }

        public TileMemento(TileCategory tileType, bool isKnown)
        {
            TileType = tileType;
            IsKnown = isKnown;
        }
    }
}