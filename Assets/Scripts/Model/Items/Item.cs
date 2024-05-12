#nullable enable
using Cysharp.Threading.Tasks;
using Scripts.Data;
using Scripts.Model.Action;
using Scripts.Model.Characters.Effect;
using Scripts.Utilities;
using UnityEngine;

namespace Scripts.Model.Items
{
    public class Item
    {
        public readonly Sprite Icon;
        public readonly Skill Skill;
        public Item(ItemData data)
        {
            Icon = data.Icon;
            Skill = new Skill(data.Skill);
        }
        public async UniTask Use(IActor actor, Direction8 direction)
        {
            await Skill.Use(actor, direction);
        }
        public float Evaluate(IActor actor, Direction8 direction)
        {
            return Skill.Evaluate(actor, direction);
        }
    }
}
