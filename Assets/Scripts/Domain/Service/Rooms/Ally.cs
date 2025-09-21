#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Domain.Service.Logs;

namespace Domain.Service.Rooms
{
    public class Ally : PlayerEvent
    {
        public Ally(ICharacter character, EnemyBehavior behavior) : base(
            null,
            new List<PlayerChoiceEvent>
            {
                new(
                    "渡す",
                    (player, map) => character.CanUseItem && character.IsAlly(player.Character),
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
                                GameLog.Add(character.Entity.IsVisible,
                                    $"{character.GetName(player)}に{item.GetName(player, map.ItemPlaceholders)}を渡した。");
                            }
                            else
                            {
                                GameLog.Add(character.Entity.IsVisible, $"{character.GetName(player)}は{item.GetName(player, map.ItemPlaceholders)}を持てない。");
                            }
                        }
                    }
                ),
                new(
                    "一緒に行動",
                    (player, map) => character.IsAlly(player.Character),
                    (gameManager, map) =>
                    {
                        behavior.BehaviorData.ChaseLeader = true;
                        behavior.BehaviorData.PrioritizeEnemiesOverLeaders = false;
                        return UniTask.CompletedTask;
                    }),
                new(
                    "敵優先",
                    (player, map) => character.IsAlly(player.Character),
                    (gameManager, map) =>
                    {
                        behavior.BehaviorData.ChaseLeader = true;
                        behavior.BehaviorData.PrioritizeEnemiesOverLeaders = true;
                        return UniTask.CompletedTask;
                    }),
                new(
                    "自由行動",
                    (player, map) => character.IsAlly(player.Character),
                    (gameManager, map) =>
                    {
                        behavior.BehaviorData.ChaseLeader = false;
                        return UniTask.CompletedTask;
                    }
                )
            }
        ) {}
    }
}