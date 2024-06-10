#nullable enable
using Cysharp.Threading.Tasks;
using Data;
using Data.Character;
using Model.Domain.Action;
using Model.Domain.Effect;
using R3;
using UnityEngine;
using Utilities;

namespace Model.Domain.Items
{
    public class Item : ISerializable<ItemMemento>
    {
        private readonly ReactiveProperty<int> _remainingUses;
        public readonly bool EffectsOnThrow;
        public readonly bool EffectsOnUse;
        public readonly Sprite Icon;
        public readonly string Info;
        public readonly string Name;
        public readonly Skill SkillOnUse;
        public readonly Skill SkillOnThrow;

        public Item(ItemData data)
        {
            Name = data.Name;
            Icon = data.Icon;
            EffectsOnUse = data.EffectsOnUse;
            EffectsOnThrow = data.EffectsOnThrow;
            SkillOnUse = new Skill(data.SkillOnUse);
            SkillOnThrow = new Skill(data.SkillOnThrow);
            _remainingUses = new ReactiveProperty<int>(data.UsageLimit);
            Info = data.Info();
        }

        public Item(ItemMemento data)
        {
            Name = data.Name;
            Icon = data.Icon;
            EffectsOnUse = data.EffectsOnUse;
            EffectsOnThrow = data.EffectsOnThrow;
            SkillOnUse = new Skill(data.SkillOnUse);
            SkillOnThrow = new Skill(data.SkillOnThrow);
            _remainingUses = new ReactiveProperty<int>(data.RemainingUses);
            Info = data.Info;
        }

        public ItemMemento Serialize()
        {
            return new ItemMemento(
                Name,
                Icon,
                EffectsOnUse,
                EffectsOnThrow,
                _remainingUses.CurrentValue,
                SkillOnUse.Serialize(),
                SkillOnThrow.Serialize(),
                Info
            );
        }

        public bool IsDisabled => _remainingUses.CurrentValue <= 0;
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUses;

        public async UniTask Use(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            _remainingUses.Value -= 1;
            await SkillOnUse.Use(actor, position, direction, world);
        }

        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            return SkillOnUse.Evaluate(actor, position, direction, world);
        }
    }
}