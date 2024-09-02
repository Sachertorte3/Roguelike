#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Item;

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
            {
                ItemEffect = _itemEffect
            };
        }

        public static ItemTargetSkillMemento Build(IItemEffect itemEffect)
        {
            return new ItemTargetSkillMemento
            {
                ItemEffect = itemEffect
            };
        }

        public async UniTask<ISkillResult> Use(IActor actor, IItem item)
        {
            var disabledItemIndexes = _itemEffect.GetDisabledItemIndexes(actor.Inventory);
            disabledItemIndexes = disabledItemIndexes.Append(actor.Inventory.GetItemIndex(item));
            var selectedItem = await actor.ItemSelecter.SelectItem(actor.Inventory, disabledItemIndexes.ToArray());
            if (selectedItem != null)
            {
                _itemEffect.Apply(selectedItem);
                return ItemTargetSkillResult.Success;
            }
            return ItemTargetSkillResult.Failed;
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