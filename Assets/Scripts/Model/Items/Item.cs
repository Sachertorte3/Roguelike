#nullable enable
using Cysharp.Threading.Tasks;
using R3;
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
        private ReactiveProperty<int> _remainingUses;
        public bool IsDisabled => _remainingUses.CurrentValue <= 0;
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUses;
        public Item(ItemData data)
        {
            Icon = data.Icon;
            Skill = new Skill(data.Skill);
            _remainingUses = new(data.UsageLimit);
        }
        public async UniTask Use(IActor actor, Vector2Int position, Direction8 direction)
        {
            _remainingUses.Value -= 1;
            await Skill.Use(actor, position, direction);
        }
        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction)
        {
            return Skill.Evaluate(actor, position, direction);
        }
    }
}
