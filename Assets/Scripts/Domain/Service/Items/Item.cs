#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Service.Effect;
using R3;
using UnityEngine;
using Utilities;
using Domain.Model.Condition;
using System.Linq;
using UnityEngine.AddressableAssets;
using Domain.Model.Effect;

namespace Domain.Service.Items
{
    public class Item : IItem
    {
        public Id<IItem> Id { get; init; }
        private readonly int _basePrice;
        private readonly int _maxUsages;
        private readonly ReactiveProperty<int> _remainingUsages;
        private readonly List<IConditionData> _conditions;
        private readonly Subject<Unit> _onItemUpdated = new();

        public Item(ItemData data) : this(Build(data)) { }
        public Item(ItemMemento data)
        {
            Id = new Id<IItem>(data.Id);
            Name = data.Name;
            Icon = Addressables.LoadAssetAsync<Sprite>($"Assets/Images/icons_full_16.png[{data.IconName}]").WaitForCompletion();
            State = data.State;
            _basePrice = data.Price;
            SkillOnUse = data.SkillOnUse.Select(skill => skill.Deserialize());
            SkillOnThrow = data.SkillOnThrow.Select(skill => skill.Deserialize());
            _isSameSkill = data.IsSameSkill;
            UseOnDeath = data.UseOnDeath;
            _maxUsages = data.MaxUsages;
            _remainingUsages = new ReactiveProperty<int>(data.RemainingUsages);
            _conditions = data.Conditions.ToList();
        }

        public string Name { get; init; }
        public Sprite Icon { get; init; }
        public ItemState State { get; private set; }
        public bool CanActivateWhenUsed => SkillOnUse.HasValue;
        public bool CanActivateWhenThrown => SkillOnThrow.HasValue;
        public Option<ISkill> SkillOnUse { get; init; }
        public Option<ISkill> SkillOnThrow { get; init; }
        private readonly bool _isSameSkill;
        private bool _usable => CanActivateWhenUsed || CanActivateWhenThrown;
        public bool UseOnDeath { get; init; }
        public int Price => Mathf.RoundToInt(_basePrice * _remainingUsages.CurrentValue / _maxUsages);
        public bool IsDisabled => _remainingUsages.CurrentValue <= 0;
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUsages;
        public IReadOnlyList<IConditionData> PassiveConditions => _conditions;
        public Observable<Unit> OnItemUpdated => _onItemUpdated;

        public ItemMemento Serialize()
        {
            return new ItemMemento
            {
                Id = Id.Value,
                Name = Name,
                IconName = Icon.name,
                State = State,
                Price = _basePrice,
                SkillOnUse = SkillOnUse.Select(skill => skill.Serialize()),
                SkillOnThrow = SkillOnThrow.Select(skill => skill.Serialize()),
                IsSameSkill = _isSameSkill,
                UseOnDeath = UseOnDeath,
                MaxUsages = _maxUsages,
                RemainingUsages = _remainingUsages.CurrentValue,
                Conditions = _conditions.ToArray()
            };
        }

        public static ItemMemento Build(ItemData data, ItemState state = ItemState.None)
        {
            var skillOnUse = data.EffectType switch
            {
                ItemEffectType.SpawnEffect => data.SpawnEffectsOnUse ? (ISkillMemento)new SpawnEffectSkill(data.SkillOnUse).Serialize() : null,
                ItemEffectType.ItemTarget => (ISkillMemento)new ItemTargetSkill(ItemTargetSkill.Build(data.ItemEffect)).Serialize(),
                _ => null
            };
            var skillOnThrow = data.SpawnEffectsOnThrow ? (ISkillMemento)new SpawnEffectSkill(data.SkillOnThrow).Serialize() : null;

            return new ItemMemento
            {
                Id = UniqueIdGenerator.Generate<IItem>().Value,
                Name = data.Name,
                IconName = data.Icon.name,
                State = state,
                Price = data.Price,
                SkillOnUse = new(skillOnUse),
                SkillOnThrow = new(skillOnThrow),
                IsSameSkill = data.IsSameSkill,
                UseOnDeath = data.UseOnDeath,
                MaxUsages = data.UsageLimit,
                RemainingUsages = data.UsageLimit,
                Conditions = data.PassiveConditions.ToArray()
            };
        }

        public void SetState(ItemState state)
        {
            State = state;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public async UniTask<bool> Use(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            var isSuccess = await ActivateWhenUse(actor, position, direction, world);
            if (isSuccess)
            {
                _remainingUsages.Value -= 1;
                if (State == ItemState.ShopItem)
                {
                    State = ItemState.UsedShopItem;
                }
                _onItemUpdated.OnNext(Unit.Default);
            }
            return isSuccess;
        }
        public async UniTask<bool> UseWhenThrown(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            var isSuccess = await ActivateWhenThrown(actor, position, direction, world);
            if (isSuccess)
            {
                _remainingUsages.Value -= 1;
                if (State == ItemState.ShopItem)
                {
                    State = ItemState.UsedShopItem;
                }
                _onItemUpdated.OnNext(Unit.Default);
            }
            return isSuccess;
        }

        public void Repair()
        {
            _remainingUsages.Value = _maxUsages;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public string Info()
        {
            var info = $"{State.GetDescription()}{Name}\n価格: {Price}\n";
            if (_usable)
            {
                if (_isSameSkill)
                {
                    info += $"[使用・投擲時]\n{SkillOnUse.Expect("SkillOnUse is null").Info()}\n";
                }
                else
                {
                    info += SkillOnUse.SelectOrDefault(skill => $"[使用時]\n{skill.Info()}\n", "");

                    info += SkillOnThrow.SelectOrDefault(skill => $"[投擲時]\n{skill.Info()}\n", "");
                }
                info += $"使用可能回数: {_remainingUsages.CurrentValue}/{_maxUsages}\n";
            }

            if (UseOnDeath)
            {
                info += "死亡時に自動的に使用される\n";
            }

            foreach (var condition in PassiveConditions)
            {
                info += $"パッシブ効果: {condition.Name}\n";
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

        public async UniTask<bool> ActivateWhenUse(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            return await SkillOnUse.SelectOrDefault(
                skill => skill.Match(
                    spawnEffectSkill => spawnEffectSkill.Use(actor, position, direction, world),
                    itemTargetSkill => itemTargetSkill.Use(actor, this)
                ),
                UniTask.FromResult(false)
            );
        }
        public async UniTask<bool> ActivateWhenThrown(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            return await SkillOnThrow.SelectOrDefault(
                skill => skill.Match(
                    spawnEffectSkill => spawnEffectSkill.Use(actor, position, direction, world),
                    itemTargetSkill => itemTargetSkill.Use(actor, this)
                ),
                UniTask.FromResult(false)
            );
        }
        public float EvaluateWhenUsed(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            return SkillOnUse.SelectOrDefault(
                skill => skill.Match(
                    spawnEffectSkill => spawnEffectSkill.Evaluate(actor, position, direction, world),
                    itemTargetSkill => itemTargetSkill.Evaluate(actor, this)
                ),
                0
            );
        }
        public float EvaluateWhenThrown(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            return SkillOnThrow.SelectOrDefault(
                skill => skill.Match(
                    spawnEffectSkill => spawnEffectSkill.Evaluate(actor, position, direction, world),
                    itemTargetSkill => itemTargetSkill.Evaluate(actor, this)
                ),
                0
            );
        }
    }
}