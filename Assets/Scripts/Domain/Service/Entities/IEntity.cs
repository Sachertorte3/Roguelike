using System;
using Domain.Model;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Entities
{
    public interface IEntity : IDisposable
    {
        public Entity Entity { get; }
        public ReadOnlyReactiveProperty<Vector2Int> Position => Entity.Position;
        public Vector2Int CurrentPosition => Entity.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => Entity.VisibleByPlayer;
        public EntityLayer Layer => Entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => Entity.OnMove;
        public Observable<Vector2Int> OnTeleport => Entity.OnTeleport;

        public void SetVisiblity(bool visiblity)
        {
            Entity.SetVisibility(visiblity);
        }
    }
}