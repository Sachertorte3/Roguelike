using R3;
using UnityEngine;
using Utilities;

namespace Model.Domain.Entities
{
    public interface IEntity
    {
        public Entity Entity { get; }
        public ReadOnlyReactiveProperty<Vector2Int> Position { get; }
        public Vector2Int CurrentPosition { get; }
        public ReadOnlyReactiveProperty<bool> Visibility { get; }
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove { get; }
        public Observable<Vector2Int> OnTeleport { get; }

        public void SetVisiblity(bool visiblity)
        {
            Entity.SetVisibility(visiblity);
        }
    }
}