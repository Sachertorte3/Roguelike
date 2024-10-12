#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
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
        private Option<Item> _item;
        private Option<EnemyData> _mimic;

        public Chest(ChestMemento memento)
        {
            _item = memento.Item.Map(i => new Item(i));
            _mimic = memento.Mimic;
            _entity = new Entity(memento.Entity);
            _events = new List<EntityEvent>
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

        private bool CanExecuteEvent()
        {
            return true;
        }

        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination, bool isThrown)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;

        private UniTask DoEvent(IGameManager gameManager, IMap mapManager)
        {
            mapManager.RemoveEventEntity(this);
            if (_item.IsSome)
            {
                mapManager.SpawnItem(_item.Value, CurrentPosition);
            }
            else
            {
                mapManager.SpawnEnemy(_mimic.Value, CurrentPosition, isSlept: false, isShiny: false);
            }

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
                if (map.CanPlace(result + direction.Vector(), false, false, false, EntityLayer.Middle))
                {
                    result += direction.Vector();
                }
                else
                {
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
                await map.ShowThrowAnimation(Icon, CurrentPosition, direction, distance, EntityLayer.Middle);
                _entity.Teleport(map.FindBlankPositionFrom(destination,
                    position => map.CanPlace(position, false, false, false, EntityLayer.Bottom, EntityLayer.Middle)));
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
                _item.Map(i => i.Serialize()),
                _mimic,
                _entity.Serialize()
            );
        }

        public static ChestMemento Build(Vector2Int position, ItemData item, string placeholder) => Build(position, new Item(item, placeholder).Serialize());
        public static ChestMemento Build(Vector2Int position, ItemMemento item)
        {
            return new ChestMemento
            (
                item,
                Entity.Build(position, EntityLayer.Middle)
            );
        }

        public static ChestMemento Build(Vector2Int position, EnemyData mimic)
        {
            return new ChestMemento
            (
                mimic,
                Entity.Build(position, EntityLayer.Middle)
            );
        }
    }
}