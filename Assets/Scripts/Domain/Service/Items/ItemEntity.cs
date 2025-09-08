#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Items
{
    internal class ItemEntity : IItemEntity
    {
        public EntityBase Entity { get; init; }

        public ItemEntity(ItemEntityMemento item)
        {
            Item = item.Item.Deserialize();
            Entity = new EntityBase(item.Entity);
        }

        public IItem Item { get; init; }

        public Sprite Icon => Item.Icon;
        public Observable<Unit> OnDisabled => Item.RemainingUses.Where(value => value <= 0).AsUnitObservable();

        public void Dispose()
        {
            Entity.Dispose();
        }

        public ItemEntityMemento Serialize()
        {
            return new ItemEntityMemento
            (
                Item.Serialize(),
                Entity.Serialize()
            );
        }

        public static Vector2Int GetThrowDestination(Vector2Int position, Direction8 direction, int distance, IMap map)
        {
            var result = position;

            for (var i = 0; i < distance; i++)
            {
                if (map.At(result + direction.Vector()).CanPlace(true, false, false, EntityLayer.Middle))
                {
                    result += direction.Vector();
                }
                else
                {
                    if (map.At(result + direction.Vector()).CanPlace(true, false, true, EntityLayer.Middle))
                    {
                        result += direction.Vector();
                    }

                    break;
                }
            }

            return result;
        }

        public static float EvaluateThrow(IItem item, Vector2Int position, IActor actor, Direction8 direction,
            int distance, IMap map)
        {
            if (!item.CanActivateWhenThrown)
                return 0;

            var destination = GetThrowDestination(position, direction, distance, map);

            return item.EvaluateWhenThrown(actor, destination, direction, map);
        }

        public async UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            var destination = GetThrowDestination(Entity.CurrentPosition, direction, distance, map);
            if (Entity.Visibility.CurrentValue && destination != Entity.CurrentPosition)
            {
                Entity.SetVisibility(false);
                await map.ShowThrowAnimation(Icon, Entity.CurrentPosition, direction, distance, EntityLayer.Middle);
                Entity.Teleport(map.FindBlankPositionFrom(destination,
                    position => map.At(position).IsBlankAndStandable(EntityLayer.Bottom)));
            }

            await map.ExecuteTrapAt(destination, actor as ICharacter);

            if (Item.CanActivateWhenThrown)
            {
                var result = await Item.UseWhenThrown(actor, destination, direction, map);
            }
        }

        ~ItemEntity()
        {
            Dispose();
        }
    }
}