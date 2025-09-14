#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Domain.Service.Logs;
using Utilities;

namespace Domain.Service.Rooms
{
    public class Ally : IPlayerEventEntity
    {
        public readonly ICharacter Character;
        public EntityBase Entity => Character.Entity;
        public readonly EnemyBehavior Behavior;
        public IPlayerEvent Event { get; init; }

        public Ally(ICharacter character, EnemyBehavior behavior, IMap map)
        {
            Character = character;
            Behavior = behavior;
            Event = new PlayerEvent(
                null,
                new List<PlayerChoiceEvent>
                {
                    new(
                        "渡す",
                        player => Character.CanUseItem && Character.IsAlly(player.Character),
                        async (gameManager, map) =>
                        {
                            var player = map.Player;
                            var focus = await player.Character.ItemSelector.SelectItem("渡すアイテムを選択してください", player.Character.Inventory, map);
                            var item = focus.GetItem(player.Character.Inventory, map);
                            if (item != null)
                            {
                                var result = character.Inventory.TryAdd(item);
                                if (result)
                                {
                                    var index = player.Character.Inventory.GetItemIndexRecursive(item);
                                    player.Character.RemoveInventory(index);
                                    GameLog.Add(Entity.IsVisible,
                                        $"{Character.GetName(player)}に{item.GetName(player, map.ItemPlaceholders)}を渡した。");
                                }
                                else
                                {
                                    GameLog.Add(Entity.IsVisible, $"{Character.GetName(player)}は{item.GetName(player, map.ItemPlaceholders)}を持てない。");
                                }
                            }
                        }
                    ),
                    new(
                        "一緒に行動",
                        player => Character.IsAlly(player.Character),
                        (gameManager, map) =>
                        {
                            Behavior.BehaviorData.ChaseLeader = true;
                            Behavior.BehaviorData.PrioritizeEnemiesOverLeaders = false;
                            return UniTask.CompletedTask;
                        }),
                    new(
                        "敵優先",
                        player => Character.IsAlly(player.Character),
                        (gameManager, map) =>
                        {
                            Behavior.BehaviorData.ChaseLeader = true;
                            Behavior.BehaviorData.PrioritizeEnemiesOverLeaders = true;
                            return UniTask.CompletedTask;
                        }),
                    new(
                        "自由行動",
                        player => Character.IsAlly(player.Character),
                        (gameManager, map) =>
                        {
                            Behavior.BehaviorData.ChaseLeader = false;
                            return UniTask.CompletedTask;
                        }
                    )
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

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return Character.BlowAway(actor, direction, distance, map);
        }
    }
}