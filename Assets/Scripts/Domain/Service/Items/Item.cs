#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Items;
using Domain.Service.Action;
using Domain.Service.Effect;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Items
{
    public class Item : IItem
    {
        private readonly int _maxUsages;
        private readonly ReactiveProperty<int> _remainingUsages;
        public bool EffectsOnThrow => SkillOnThrow != null;
        public bool EffectsOnUse => SkillOnUse != null;
        private bool _usable => EffectsOnUse || EffectsOnThrow;
        public Sprite Icon { get; init; }
        public string Name  { get; init; }
        public int Price { get; init; }
        private readonly bool _isSameSkill;
        public ISkill? SkillOnThrow { get; init; }
        public ISkill? SkillOnUse { get; init; }

        public Item(ItemData data)
        {
            Name = data.Name;
            Icon = data.Icon;
            Price = data.Price;
            if (data.EffectsOnUse)
            {
                SkillOnUse = new Skill(data.SkillOnUse);
            }
            if (data.EffectsOnThrow)
            {
                SkillOnThrow = new Skill(data.SkillOnThrow);
            }
            _maxUsages = data.UsageLimit;
            _remainingUsages = new ReactiveProperty<int>(data.UsageLimit);
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
            _maxUsages = data.MaxUsages;
            _remainingUsages = new ReactiveProperty<int>(data.RemainingUsages);
        }

        public bool IsDisabled => _remainingUsages.CurrentValue <= 0;
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUsages;

        public ItemMemento Serialize()
        {
            return new ItemMemento(
                Name,
                Icon,
                _maxUsages,
                _remainingUsages.CurrentValue,
                SkillOnUse?.Serialize(),
                SkillOnThrow?.Serialize()
            );
        }

        public async UniTask Use(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            _remainingUsages.Value -= 1;
            await SkillOnUse.Use(actor, position, direction, world);
        }

        public void Repair()
        {
            _remainingUsages.Value = _maxUsages;
        }

        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            return SkillOnUse.Evaluate(actor, position, direction, world);
        }
        public string Info()
        {
            var info = $"{Name}\n";
            if (_usable)
            {
                if (_isSameSkill)
                {
                    info += $"[使用・投擲時]\n{SkillOnUse.Info()}\n";
                }
                else
                {
                    if (SkillOnUse != null)
                    {
                        info += $"[使用時]\n{SkillOnUse.Info()}\n";
                    }
                    if (SkillOnThrow != null)
                    {
                        info += $"[投擲時]\n{SkillOnThrow.Info()}\n";
                    }
                }
                info += $"使用可能回数: {_remainingUsages.CurrentValue}/{_maxUsages}";
            }
            return info;
        }
    }
}