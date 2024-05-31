#nullable enable
using Cysharp.Threading.Tasks;
using Data;
using Model.Domain.Action;
using Model.Domain.Effect;
using R3;
using UnityEngine;
using Utilities;

namespace Model.Domain.Items
{
    public class Item
    {
        private readonly ReactiveProperty<int> _remainingUses;
        public readonly bool EffectsOnThrow;
        public readonly bool EffectsOnUse;
        public readonly Sprite Icon;
        public readonly string Info;
        public readonly string Name;
        public readonly Skill Skill;

        public Item(ItemData data)
        {
            Name = data.Name;
            Icon = data.Icon;
            EffectsOnUse = data.EffectsOnUse;
            EffectsOnThrow = data.EffectsOnThrow;
            Skill = new Skill(data.Skill);
            _remainingUses = new ReactiveProperty<int>(data.UsageLimit);
            Info = data.Info();
        }

        public bool IsDisabled => _remainingUses.CurrentValue <= 0;
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUses;

        public async UniTask Use(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            _remainingUses.Value -= 1;
            await Skill.Use(actor, position, direction, world);
        }

        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            return Skill.Evaluate(actor, position, direction, world);
        }
    }
}