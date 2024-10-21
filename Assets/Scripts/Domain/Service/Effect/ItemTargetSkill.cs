#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Item;
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

        public async UniTask<ISkillResult> Use(IHasInventory player, IItem item, ItemPlaceholders itemPlaceholders)
        {
            var selfIndex = player.Inventory.GetItemIndex(item);
            var disabledItemIndexes = _itemEffect.GetDisabledItemIndexes(player);
            disabledItemIndexes = disabledItemIndexes.Append(selfIndex);
            if (player.IsKnownItem(item))
            {
                var selectedItem = await player.ItemSelector.SelectItem(player.Inventory, disabledItemIndexes.ToArray());
                if (selectedItem != null)
                {
                    _itemEffect.Apply(player, selectedItem, itemPlaceholders);
                    return ItemTargetSkillResult.Success;
                }
            }
            else
            {
                var selectedItem = await player.ItemSelector.SelectItem(player.Inventory, new[] { selfIndex });
                if (selectedItem != null)
                {
                    var selectedItemIndex = player.Inventory.GetItemIndex(selectedItem);
                    if (disabledItemIndexes.Contains(selectedItemIndex))
                    {
                        GameLog.Add($"しかし効果はなかった。");
                    }
                    else
                    {
                        _itemEffect.Apply(player, selectedItem, itemPlaceholders);
                    }
                    return ItemTargetSkillResult.Success;
                }
            }

            return ItemTargetSkillResult.Cancelled;
        }

        public float Evaluate(IActor player, IItem item)
        {
            return 0;
        }

        public float EvaluatePrice()
        {
            return _itemEffect.EvaluatePrice();
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() => new();

        public string Info()
        {
            var info = $"効果: {_itemEffect.Info()}";
            return info;
        }
    }
}