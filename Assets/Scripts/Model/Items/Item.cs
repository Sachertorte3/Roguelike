#nullable enable
using Cysharp.Threading.Tasks;
using Data;
using Model.Action;
using Model.Characters.Effect;
using R3;
using UnityEngine;
using Utilities;

namespace Model.Items
{
    public class Item
    {
        public readonly Sprite Icon;
        public readonly string Info;
        public readonly Skill Skill;
        private readonly ReactiveProperty<int> _remainingUses;

        public Item(ItemData data)
        {
            Icon = data.Icon;
            Skill = new Skill(data.Skill);
            _remainingUses = new ReactiveProperty<int>(data.UsageLimit);
            Info = data.Info();
        }

        public bool IsDisabled => _remainingUses.CurrentValue <= 0;
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUses;

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