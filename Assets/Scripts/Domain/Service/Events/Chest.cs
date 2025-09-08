#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Items;
using Domain.Service.Logs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Events
{
    public class Chest : ISerializable<ChestMemento>, IPlayerEventEntity, IIconEntity
    {
        public EntityBase Entity { get; init; }
        private Option<IItem> _item;
        private Option<EnemyData> _mimic;

        public Chest(ChestMemento memento)
        {
            _item = memento.Item.Map(i => i.Deserialize());
            _mimic = memento.Mimic;
            Entity = new EntityBase(memento.Entity);
            Event = new PlayerEvent(
                "宝箱を見つけた",
                new List<PlayerChoiceEvent>
                {
                    new(
                        "開ける",
                        player => true,
                        async (gameManager, map) => { await DoEvent(map); }
                    )
                }
            );
        }

        public Sprite Icon => Addressables.LoadAssetAsync<Sprite>("Assets/Images/Monsters/ChestA.png[Chest_0]")
            .WaitForCompletion();

        public IPlayerEvent Event { get; init; }

        private UniTask DoEvent(IMap map)
        {
            map.RemoveEventEntity(this);
            if (_item.IsSome)
            {
                if (map.Player.Character.TryAddToInventory(_item.Value))
                {
                    GameLog.Add(
                        $"{map.Player.Character.GetName(map.Player)}は{_item.Value.GetName(map.Player, map.ItemPlaceholders)}を手に入れた");
                }
                else
                {
                    GameLog.Add($"{_item.Value.GetName(map.Player, map.ItemPlaceholders)}を拾えなかった");
                    map.SpawnItem(_item.Value, Entity.CurrentPosition);
                }
            }
            else
            {
                map.SpawnEnemy(_mimic.Value, Entity.CurrentPosition, isSlept: false, isShiny: false);
            }

            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public static Vector2Int GetThrowDestination(Vector2Int position, Direction8 direction, int distance, IMap map)
        {
            var result = position;

            for (var i = 0; i < distance; i++)
            {
                if (map.At(result + direction.Vector()).CanPlace(false, false, false, EntityLayer.Middle))
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
            var destination = GetThrowDestination(Entity.CurrentPosition, direction, distance, map);
            if (Entity.Visibility.CurrentValue && destination != Entity.CurrentPosition)
            {
                Entity.SetVisibility(false);
                await map.ShowThrowAnimation(Icon, Entity.CurrentPosition, direction, distance, EntityLayer.Middle);
                Entity.Teleport(map.FindBlankPositionFrom(destination,
                    position => map.At(position)
                        .CanPlace(false, false, false, EntityLayer.Bottom, EntityLayer.Middle)));
            }
        }

        public ChestMemento Serialize()
        {
            return new ChestMemento
            (
                _item.Map(i => i.Serialize()),
                _mimic,
                Entity.Serialize()
            );
        }

        public static ChestMemento Build(Vector2Int position, IItemData item)
        {
            return Build(position, item.Build());
        }

        public static ChestMemento Build(Vector2Int position, IItemMemento item)
        {
            return new ChestMemento
            (
                item,
                EntityBase.Build(position, EntityLayer.Middle)
            );
        }

        public static ChestMemento Build(Vector2Int position, EnemyData mimic)
        {
            return new ChestMemento
            (
                mimic,
                EntityBase.Build(position, EntityLayer.Middle)
            );
        }
    }
}