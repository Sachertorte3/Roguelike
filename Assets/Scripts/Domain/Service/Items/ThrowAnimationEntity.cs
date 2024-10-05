#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.Setting;
using Domain.Service.Entities;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Items
{
    public class ThrowAnimationEntity : IEntity
    {
        private readonly Entity _entity;
        public readonly Sprite Icon;
        public Id<IEntity> Id => _entity.Id;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination, bool isThrown)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public Observable<Unit> OnDestroyed => _entity.OnDestroyed;

        public ThrowAnimationEntity(Vector2Int position, Sprite icon)
        {
            _entity = new Entity(Entity.Build(position, EntityLayer.Middle));
            Icon = icon;
        }

        public async UniTask<Vector2Int> Throw(Direction8 direction, IMap map, params EntityLayer[] canHitLayer)
        {
            while (map.IsBlank(CurrentPosition + direction.Vector(), canHitLayer))
            {
                await _entity.Move(direction, Settings.ThrowMilliseconds.Value, true);
            }

            if (map.IsPassableOnMap(CurrentPosition + direction.Vector()))
            {
                await _entity.Move(direction, Settings.ThrowMilliseconds.Value, true);
            }

            return CurrentPosition;
        }

        public void SetVisibility(bool visibility)
        {
            _entity.SetVisibility(visibility);
        }

        public void Destroy()
        {
            _entity.Destroy();
        }

        public void Dispose()
        {
            _entity.Dispose();
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public void Teleport(Vector2Int position)
        {
            _entity.Teleport(position);
        }
    }
}