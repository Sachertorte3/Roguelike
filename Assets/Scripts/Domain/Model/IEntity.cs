using System;
using Domain.Model;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Entities
{
    public interface IEntity : IDisposable
    {
        public ReadOnlyReactiveProperty<Vector2Int> Position { get; }
        public Vector2Int CurrentPosition { get; }
        public ReadOnlyReactiveProperty<bool> Visibility { get; }
        public EntityLayer Layer { get; }
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove { get; }
        public Observable<Vector2Int> OnTeleport { get; }
        public Observable<Unit> OnDestroyed { get; }
        public void SetVisiblity(bool visiblity);
    }
}