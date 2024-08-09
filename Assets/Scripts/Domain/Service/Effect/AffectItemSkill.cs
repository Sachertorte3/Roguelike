#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using UnityEngine;
using Utilities;
using Domain.Model.Action;
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

        public async UniTask<bool> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            var item = await actor.ItemSelecter.SelectItem(actor.Inventory);
            if (item != null)
                _itemEffect.Apply(item);
            return item != null;
        }

        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            return 0;
        }

        public string Info()
        {
            var info = $"効果: {_itemEffect.Info()}";
            return info;
        }
    }
}