#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Item;
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
                    (player, map) =>
                        character.CanUseItem
                        && character.IsAlly(player.Character)
                        && player.Character.Inventory.HasEmptySpace(),
                    async (gameManager, map) =>
                    {
                        var player = map.Player;
                        var disabledItemIndexes = new List<ItemFocus>();
                        foreach (var inventoryIndex in player.Character.Inventory.AllIndexesRecursive)
                        {
                            if (!player.Character.Inventory.CanRemove(inventoryIndex))
                            {
                                disabledItemIndexes.Add(inventoryIndex);
                            }
                        }
                        var focus = await player.Character.SelectItem("渡すアイテムを選択してください", disabledItemIndexes.ToArray());
                        if (focus.IsOnItem(player.Character.Inventory, map, out var item))
                        {
                            if (character.Inventory.CanAddToEmpty(item) && player.Character.Inventory.CanRemove(focus))
                            {
                                character.Inventory.AddToEmpty(item);
                                player.Character.Inventory.Remove(focus);
                                GameLog.Add(character.Entity.IsVisible,
                                    $"{character.GetName(player)}に{item.GetName(player, map.ItemPlaceholders)}を渡した。");
                            }
                            else
                            {
                                GameLog.Add(character.Entity.IsVisible, $"{item.GetName(player, map.ItemPlaceholders)}を渡せなかった。");
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
        )
        { }
    }
}