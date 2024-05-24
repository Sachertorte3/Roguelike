#nullable enable
using Cysharp.Threading.Tasks;
using Data;
using Model.Action;
using Model.Domain;
using Model.Effect;
using R3;
using UnityEngine;
using Utilities;

namespace Model.Items
{
    public class Item
    {
        public readonly Sprite Icon;
        public readonly bool EffectsOnUse;
        public readonly bool EffectsOnThrow;
        public readonly Skill Skill;
        private readonly ReactiveProperty<int> _remainingUses;
        public readonly string Info;

        public Item(ItemData data)
        {
            Icon = data.Icon;
            EffectsOnUse = data.EffectsOnUse;
            EffectsOnThrow = data.EffectsOnThrow;
            Skill = new Skill(data.Skill);
            _remainingUses = new ReactiveProperty<int>(data.UsageLimit);
            Info = data.Info();
        }

        public bool IsDisabled => _remainingUses.CurrentValue <= 0;
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUses;

        public async UniTask Use(IActor actor, Vector2Int position, Direction8 direction, IWorld world)
        {
            _remainingUses.Value -= 1;
            await Skill.Use(actor, position, direction, world);
        }

        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IWorld world)
        {
            return Skill.Evaluate(actor, position, direction, world);
        }
    }
}