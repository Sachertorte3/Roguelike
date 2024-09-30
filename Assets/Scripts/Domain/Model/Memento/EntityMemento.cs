#nullable enable
using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class EntityMemento
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public Vector2Int Position { get; private set; }
        [field: SerializeField] public EntityLayer Layer { get; private set; }
        public EntityMemento(string id, Vector2Int position, EntityLayer layer)
        {
            Id = id;
            Position = position;
            Layer = layer;
        }
        public EntityMemento CopyWith(string? id=null, Vector2Int? position=null, EntityLayer? layer=null)
        {
            return new EntityMemento(
                id ?? Id,
                position ?? Position,
                layer ?? Layer
            );
        }
    }
}