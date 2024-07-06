#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Items;
using Domain.Model.Message;
using Domain.Service.Action;
using Domain.Service.Effect;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Items
{
    public class Item : IItem
    {
        public Id<IItem> Id { get; init; }
        private readonly bool _isSameSkill;
        private readonly int _maxUsages;
        private readonly ReactiveProperty<int> _remainingUsages;
        private readonly Subject<Unit> _onItemUpdated = new();

        public Item(ItemData data, ItemState state = ItemState.None)
        {
            Id = UniqueIdGenerator.Generate<IItem>();
            Name = data.Name;
            Icon = data.Icon;
            State = state;
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
            Id = new Id<IItem>(data.Id);
            Name = data.Name;
            Icon = data.Icon;
            State = data.State;
            Price = data.Price;
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

        public string Name { get; init; }
        public Sprite Icon { get; init; }
        public ItemState State { get; private set; }
        public bool EffectsOnThrow => SkillOnThrow != null;
        public bool EffectsOnUse => SkillOnUse != null;
        public int Price { get; init; }
        public ISkill? SkillOnThrow { get; init; }
        public ISkill? SkillOnUse { get; init; }

        private bool _usable => EffectsOnUse || EffectsOnThrow;
        public bool IsDisabled => _remainingUsages.CurrentValue <= 0;
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUsages;
        public Observable<Unit> OnItemUpdated => _onItemUpdated;

        public ItemMemento Serialize()
        {
            return new ItemMemento(
                Id.Value,
                Name,
                Icon,
                State,
                Price,
                _maxUsages,
                _remainingUsages.CurrentValue,
                SkillOnUse?.Serialize(),
                SkillOnThrow?.Serialize()
            );
        }

        public void SetState(ItemState state)
        {
            State = state;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public async UniTask Use(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            _remainingUsages.Value -= 1;
            if (State == ItemState.ShopItem)
            {
                State = ItemState.UsedShopItem;
            }
            _onItemUpdated.OnNext(Unit.Default);
            await SkillOnUse.Use(actor, position, direction, world);
        }

        public void Repair()
        {
            _remainingUsages.Value = _maxUsages;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            return SkillOnUse.Evaluate(actor, position, direction, world);
        }

        public string Info()
        {
            var info = $"{State.GetDescription()}{Name}\n価格: {Price}\n";
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

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Id.Value == ((Item)obj).Id.Value;
        }
        
        public override int GetHashCode()
        {
            return Id.Value;
        }
    }
}