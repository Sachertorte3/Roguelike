#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Memento;

namespace Domain.Service.Effect
{
    public class ItemTargetSkill : ISerializable<ItemTargetSkillMemento>, ISkill
    {
        private readonly IItemEffect _itemEffect;

        public ItemTargetSkill(ItemTargetSkillMemento memento)
        {
            _itemEffect = memento.ItemEffect;
        }

        public ItemTargetSkillMemento Serialize()
        {
            return new ItemTargetSkillMemento
            (
                itemEffect: _itemEffect
            );
        }

        public static ItemTargetSkillMemento Build(IItemEffect itemEffect)
        {
            return new ItemTargetSkillMemento
            (
                itemEffect: itemEffect
            );
        }

        public async UniTask<ISkillResult> Use(IActor actor, IItem item)
        {
            var disabledItemIndexes = _itemEffect.GetDisabledItemIndexes(actor.Inventory);
            disabledItemIndexes = disabledItemIndexes.Append(actor.Inventory.GetItemIndex(item));
            var selectedItem = await actor.ItemSelector.SelectItem(actor.Inventory, disabledItemIndexes.ToArray());
            if (selectedItem != null)
            {
                _itemEffect.Apply(selectedItem);
                return ItemTargetSkillResult.Success;
            }
            return ItemTargetSkillResult.Cancelled;
        }

        public float Evaluate(IActor actor, IItem item)
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