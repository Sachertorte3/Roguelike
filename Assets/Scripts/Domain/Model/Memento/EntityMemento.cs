#nullable enable
using System;
using Domain.Model.Entity;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class EntityMemento
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public Vector2Int Position { get; private set; }
        [field: SerializeField] public EntityLayer Layer { get; private set; }
        [field: SerializeField] public bool IsDestroyed { get; private set; }

        public EntityMemento(string id, Vector2Int position, EntityLayer layer, bool isDestroyed)
        {
            Id = id;
            Position = position;
            Layer = layer;
            IsDestroyed = isDestroyed;
        }

        public EntityMemento CopyWith(string? id = null, Vector2Int? position = null, EntityLayer? layer = null, bool? isDestroyed = null)
        {
            return new EntityMemento(
                id ?? Id,
                position ?? Position,
                layer ?? Layer,
                isDestroyed ?? IsDestroyed
            );
        }
    }
}