#nullable enable
using R3;
using Scripts.Model.Entities;
using Scripts.Utilities;
using UnityEngine;

namespace Scripts.Model.Items
{
    public class ItemEntity
    {
        public readonly Item Item;
        private readonly Entity _entity;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public ItemEntity(Vector2Int spawnPosition, Item item)
        {
            Item = item;
            _entity = new Entity(spawnPosition);
        }
    }
}
