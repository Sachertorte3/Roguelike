using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Model
{
    public interface IEntity : IDisposable
    {
        public Id<IEntity> Id { get; }
        public ReadOnlyReactiveProperty<Vector2Int> Position { get; }
        public Vector2Int CurrentPosition { get; }
        public ReadOnlyReactiveProperty<bool> Visibility { get; }
        public EntityLayer Layer { get; }
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove { get; }
        public Observable<Vector2Int> OnTeleport { get; }
        public Observable<Unit> OnDestroyed { get; }
        public void SetVisibility(bool visibility);
        public void Destroy();
        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map);
        public void Teleport(Vector2Int position);
    }
}