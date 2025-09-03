#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Logs;

namespace Domain.Service.Effect
{
    public class ItemTargetSkill : ISerializable<ItemTargetSkillMemento>, ISkill
    {
        private readonly IItemEffect _itemEffect;
        public bool IsDirectional => false;

        public ItemTargetSkill(ItemTargetSkillMemento memento)
        {
            _itemEffect = memento.ItemEffect;
        }

        public ItemTargetSkillMemento Serialize()
        {
            return new ItemTargetSkillMemento
            (
                _itemEffect
            );
        }

        public static ItemTargetSkillMemento Build(IItemEffect itemEffect)
        {
            return new ItemTargetSkillMemento
            (
                itemEffect
            );
        }

        private ItemFocus GetItemIndex(IPlayer player, IItem item, IMap map)
        {
            var selfIndex = player.Character.Inventory.GetItemIndexRecursive(item);
            if (selfIndex != null)
            {
                return selfIndex;
            }

            var groundItem = map.Items.At(player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
            if (item == groundItem)
            {
                return ItemFocus.GroundItem;
            }

            throw new Exception("ItemTargetSkill: Item not found in inventory or ground.");
        }

        public async UniTask<ISkillResult> Use(IPlayer player, IItem item, IMap map)
        {
            var selfIndex = GetItemIndex(player, item, map);

            var disabledItemIndexes = new List<ItemFocus>();
            foreach (var index in player.Character.Inventory.AllIndexesRecursive)
            {
                var inventoryItem = player.Character.Inventory.GetItem(index);
                if (inventoryItem == null || !_itemEffect.CanApplyTo(player, inventoryItem))
                {
                    disabledItemIndexes.Add(index);
                }
            }

            var groundItem = map.Items.At(player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
            if (groundItem == null || !_itemEffect.CanApplyTo(player, groundItem))
            {
                disabledItemIndexes.Add(ItemFocus.GroundItem);
            }

            disabledItemIndexes.Add(selfIndex);
            if (player.Character.IsKnownItem(item))
            {
                var selectedItem = await player.Character.ItemSelector.SelectItem("適応するアイテムを選択してください", player.Character.Inventory, map,
                    disabledItemIndexes.ToArray());
                if (selectedItem != null)
                {
                    _itemEffect.Apply(player, selectedItem, map.ItemPlaceholders);
                    return ItemTargetSkillResult.Success;
                }
            }
            else
            {
                var selectedItem =
                    await player.Character.ItemSelector.SelectItem("適応するアイテムを選択してください", player.Character.Inventory, map, selfIndex);
                if (selectedItem != null)
                {
                    var selectedItemIndex = player.Character.Inventory.GetItemIndexRecursive(selectedItem);
                    if (disabledItemIndexes.Contains(selectedItemIndex))
                    {
                        GameLog.Add("しかし効果はなかった。");
                    }
                    else
                    {
                        _itemEffect.Apply(player, selectedItem, map.ItemPlaceholders);
                    }

                    return ItemTargetSkillResult.Success;
                }
            }

            return ItemTargetSkillResult.Cancelled;
        }

        public float Evaluate(IPlayer player, IItem item)
        {
            return 0;
        }

        public float EvaluatePrice()
        {
            return _itemEffect.EvaluatePrice();
        }

        public List<UpgradeData> GetUpgrades()
        {
            return new List<UpgradeData>();
        }

        public Dictionary<string, IHasUpgrades> GetChildren()
        {
            return new Dictionary<string, IHasUpgrades>();
        }

        public string Info()
        {
            return _itemEffect.Info();
        }
    }
}