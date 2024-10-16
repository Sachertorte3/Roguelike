#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Rooms
{
    public class Ally : IEventEntity
    {
        public readonly ICharacter Character;
        public readonly EnemyBehavior Behavior;
        public IEvent Event { get; init; }

        public Ally(ICharacter character, EnemyBehavior behavior, IMap map)
        {
            Character = character;
            Behavior = behavior;
            Event = new PlayerEvent(
                null,
                true,
                new List<PlayerChoiceEvent>
                {
                    new PlayerChoiceEvent(
                        "渡す",
                        (player) => Character.CanUseItem && Character.IsAlly(player),
                        async (gameManager, map) =>
                        {
                            var player = map.Player;
                            var item = await player.ItemSelector.SelectItem(player.Inventory);
                            if (item != null)
                            {
                                var result = character.Inventory.TryAdd(item);
                                if (result)
                                {
                                    var index = player.Inventory.GetItemIndex(item);
                                    player.ReplaceInventory(null, index);
                                    GameLog.Add($"{Character.GetName(player)}に{item.GetName(player, map.ItemDatabase)}を渡した。");
                                }
                                else
                                {
                                    GameLog.Add($"{Character.GetName(player)}はこれ以上アイテムを持てない。");
                                }
                            }
                        }
                    ),
                new PlayerChoiceEvent(
                    "一緒に行動",
                    (player) => Character.IsAlly(player),
                    (gameManager, map) =>
                    {
                        Behavior.BehaviorData.PrioritizeEnemiesOverLeaders = false;
                        return UniTask.CompletedTask;
                    }),
                new PlayerChoiceEvent(
                    "敵優先",
                    (player) => Character.IsAlly(player),
                    (gameManager, map) => {
                        Behavior.BehaviorData.PrioritizeEnemiesOverLeaders = true;
                        return UniTask.CompletedTask;
                    })
                }
            );
        }

        public void Dispose()
        {
            Character.Dispose();
        }

        ~Ally()
        {
            Dispose();
        }

        public Id<IEntity> Id => Character.Id;
        public ReadOnlyReactiveProperty<Vector2Int> Position => Character.Position;
        public Vector2Int CurrentPosition => Character.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => Character.Visibility;
        public EntityLayer Layer => Character.Layer;
        public Observable<(Direction8 direction, Vector2Int destination, bool isThrown)> OnMove => Character.OnMove;
        public Observable<Vector2Int> OnTeleport => Character.OnTeleport;
        public Observable<Unit> OnDestroyed => Character.OnDestroyed;

        public void SetVisibility(bool visibility)
        {
            Character.SetVisibility(visibility);
        }

        public void Destroy()
        {
            Character.Destroy();
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return Character.BlowAway(actor, direction, distance, map);
        }

        public void Teleport(Vector2Int position)
        {
            Character.Teleport(position);
        }
    }
}