#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.Setting;
using UnityEngine;
using Utilities;

namespace Domain.Service.Items
{
    public class ThrowAnimationEntity : IEntity
    {
        public Entity Entity { get; init; }
        public readonly Sprite Icon;

        public ThrowAnimationEntity(Vector2Int position, Sprite icon)
        {
            Entity = new Entity(Entity.Build(position, EntityLayer.Middle));
            Icon = icon;
        }

        public async UniTask<Vector2Int> Throw(Direction8 direction, IMap map, int distance, params EntityLayer[] canHitLayer)
        {
            for (var i = 0; i < distance; i++)
            {
                if (map.At(Entity.CurrentPosition + direction.Vector()).IsBlank(canHitLayer))
                {
                    await Entity.Move(direction, Settings.ThrowMilliseconds.Value, true);
                }
                else if (map.At(Entity.CurrentPosition + direction.Vector()).IsPassableOnMap())
                {
                    await Entity.Move(direction, Settings.ThrowMilliseconds.Value, true);
                    break;
                }
                else
                {
                    break;
                }
            }

            return Entity.CurrentPosition;
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }
    }
}