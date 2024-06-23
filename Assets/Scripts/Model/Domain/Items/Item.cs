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
    public class Item : ISerializable<ItemMemento>, IHasInfo
    {
        private readonly ReactiveProperty<int> _remainingUses;
        public bool EffectsOnThrow => SkillOnThrow != null;
        public bool EffectsOnUse => SkillOnUse != null;
        public readonly Sprite Icon;
        private readonly string _info;
        public readonly string Name;
        public readonly Skill? SkillOnThrow;
        public readonly Skill? SkillOnUse;

        public Item(ItemData data)
        {
            Name = data.Name;
            Icon = data.Icon;
            if (data.EffectsOnUse)
            {
                SkillOnUse = new Skill(data.SkillOnUse);
            }
            if (data.EffectsOnThrow)
            {
                SkillOnThrow = new Skill(data.SkillOnThrow);
            }
            _remainingUses = new ReactiveProperty<int>(data.UsageLimit);
            _info = data.Info();
        }

        public Item(ItemMemento data)
        {
            Name = data.Name;
            Icon = data.Icon;
            if (data.SkillOnUse != null)
            {
                SkillOnUse = new Skill(data.SkillOnUse);
            }
            if (data.SkillOnThrow != null)
            {
                SkillOnThrow = new Skill(data.SkillOnThrow);
            }
            _remainingUses = new ReactiveProperty<int>(data.RemainingUses);
            _info = data.Info;
        }

        public bool IsDisabled => _remainingUses.CurrentValue <= 0;
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUses;

        public ItemMemento Serialize()
        {
            return new ItemMemento(
                Name,
                Icon,
                _remainingUses.CurrentValue,
                SkillOnUse?.Serialize(),
                SkillOnThrow?.Serialize(),
                _info
            );
        }

        public async UniTask Use(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            _remainingUses.Value -= 1;
            await SkillOnUse.Use(actor, position, direction, world);
        }

        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            return SkillOnUse.Evaluate(actor, position, direction, world);
        }
        public string Info() => _info;
    }
}