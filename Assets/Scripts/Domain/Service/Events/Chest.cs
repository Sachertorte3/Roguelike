using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Entities;
using Domain.Service.Items;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Domain.Service.Events
{
    public class Chest : ISerializable<ChestMemento>, IIconEventEntity
    {
        private Entity _entity;
        private Item _item;

        public Chest(ChestMemento memento)
        {
            _item = new Item(memento.Item);
            _entity = new Entity(memento.Entity);
            _events = new()
            {
                new EntityEvent("開ける", CanExecuteEvent, DoEvent)
            };
        }

        public Sprite Icon => Addressables.LoadAssetAsync<Sprite>("Assets/Images/Monsters/ChestA.png[Chest_0]")
            .WaitForCompletion();

        public string ChoiceMessage => "宝箱を見つけた";
        private readonly List<EntityEvent> _events;
        public IReadOnlyList<EntityEvent> Events => _events;
        public bool CanBeCanceled => true;
        public Id<IEntity> Id => _entity.Id;
        public Observable<Unit> OnDestroyed => _entity.OnDestroyed;
        private bool CanExecuteEvent() => true;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;

        private UniTask DoEvent(IGameManager gameManager, IMap mapManager)
        {
            mapManager.SpawnItem(_item, CurrentPosition);
            mapManager.RemoveEventEntity(this);
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            _entity.Dispose();
        }

        public void SetVisibility(bool visibility)
        {
            _entity.SetVisibility(visibility);
        }

        public void Destroy()
        {
            _entity.Destroy();
        }

        public static Vector2Int GetThrowDestination(Vector2Int position, Direction8 direction, int distance, IMap map)
        {
            var result = position;

            for (var i = 0; i < distance; i++)
            {
                if (map.IsPassable(result + direction.Vector()))
                {
                    result += direction.Vector();
                }
                else
                {
                    if (map.IsPassableOnMap(result + direction.Vector()))
                    {
                        result += direction.Vector();
                    }
                    break;
                }
            }

            return result;
        }

        public async UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            var destination = GetThrowDestination(CurrentPosition, direction, distance, map);
            if (_entity.VisibleByPlayer.CurrentValue && destination != CurrentPosition)
            {
                _entity.SetVisibility(false);
                await map.ShowThrowAnimation(Icon, CurrentPosition, direction, EntityLayer.Middle);
                _entity.Teleport(map.FindBlankPositionFrom(destination, position => map.IsBlank(position, EntityLayer.Bottom, EntityLayer.Middle)));
            }
        }

        public void Teleport(Vector2Int position)
        {
            _entity.Teleport(position);
        }

        public ChestMemento Serialize()
        {
            return new ChestMemento
            (
                item: _item.Serialize(),
                entity: _entity.Serialize()
            );
        }

        public static ChestMemento Build(Vector2Int position, ItemData item)
        {
            return new ChestMemento
            (
                item: new Item(item).Serialize(),
                entity: Entity.Build(position, EntityLayer.Middle)
            );
        }
    }
}