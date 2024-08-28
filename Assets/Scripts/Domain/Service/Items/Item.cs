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
        private string _name;
        private readonly int _basePrice;
        private int _upgradeCount;
        private int _maxUsages;
        private readonly ReactiveProperty<int> _remainingUsages;
        private readonly Option<ISkill> _skillOnUse;
        private readonly Option<ISkill> _skillOnThrow;
        private readonly List<IConditionData> _conditions;
        private readonly Subject<Unit> _onItemUpdated = new();

        public Item(ItemData data) : this(Build(data)) { }
        public Item(ItemMemento data)
        {
            Id = new Id<IItem>(data.Id);
            _name = data.Name;
            Icon = Addressables.LoadAssetAsync<Sprite>($"Assets/Images/icons_full_16.png[{data.IconName}]").WaitForCompletion();
            _upgradeCount = data.UpgradeCount;
            State = data.State;
            _basePrice = data.Price;
            _skillOnUse = data.SkillOnUse.Select(skill => skill.Deserialize());
            _skillOnThrow = data.SkillOnThrow.Select(skill => skill.Match(
                spawnEffectSkillMemento =>
                {
                    if (data.HasSameEffect)
                    {
                        return _skillOnUse.Expect("SkillOnUse is null").Match(
                            spawnEffectSkill => spawnEffectSkill.CreateSkillWithEffect(spawnEffectSkillMemento),
                            itemTargetSkill => throw new Exception("SkillOnUse is not SpawnEffectSkill")
                        );
                    }
                    return new SpawnEffectSkill(spawnEffectSkillMemento);
                },
                itemTargetSkillMemento => (ISkill)new ItemTargetSkill(itemTargetSkillMemento)
            ));
            _hasSameEffect = data.HasSameEffect;
            _hasSameSkill = data.HasSameSkill;
            UseOnDeath = data.UseOnDeath;
            _maxUsages = data.MaxUsages;
            _remainingUsages = new ReactiveProperty<int>(data.RemainingUsages);
            _conditions = data.Conditions.ToList();
        }

        public string Name => _upgradeCount > 0 ? $"{_name} +{_upgradeCount}" : _name;
        public Sprite Icon { get; init; }
        public ItemState State { get; private set; }
        public bool CanActivateWhenUsed => SkillOnUse.HasValue;
        public bool CanActivateWhenThrown => SkillOnThrow.HasValue;
        public Option<ISkill> SkillOnUse => _skillOnUse;
        public Option<ISkill> SkillOnThrow => _hasSameSkill ? _skillOnUse : _skillOnThrow;
        private readonly bool _hasSameEffect;
        private readonly bool _hasSameSkill;
        private bool _usable => CanActivateWhenUsed || CanActivateWhenThrown;
        public bool UseOnDeath { get; init; }
        public int Price => Mathf.RoundToInt(_basePrice * _remainingUsages.CurrentValue / _maxUsages);
        public bool IsDisabled => _remainingUsages.CurrentValue <= 0;
        public int MaxUsages => _maxUsages;
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUsages;
        public IReadOnlyList<IConditionData> PassiveConditions => _conditions;
        public Observable<Unit> OnItemUpdated => _onItemUpdated;

        public ItemMemento Serialize()
        {
            return new ItemMemento
            {
                Id = Id.Value,
                Name = _name,
                IconName = Icon.name,
                UpgradeCount = _upgradeCount,
                State = State,
                Price = _basePrice,
                SkillOnUse = _skillOnUse.Select(skill => skill.Serialize()),
                SkillOnThrow = _skillOnThrow.Select(skill => skill.Serialize()),
                HasSameEffect = _hasSameEffect,
                HasSameSkill = _hasSameSkill,
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
                ItemEffectType.SpawnEffect => data.SpawnEffectsOnUse ? (ISkillMemento)SpawnEffectSkill.Build(data.SkillOnUse) : null,
                ItemEffectType.ItemTarget => (ISkillMemento)new ItemTargetSkill(ItemTargetSkill.Build(data.ItemEffect)).Serialize(),
                _ => null
            };
            ISkillMemento? skillOnThrow;
            if (data.IsSameSkill)
            {
                skillOnThrow = null;
            }
            else
            {
                skillOnThrow = data.SpawnEffectsOnThrow ? (ISkillMemento)SpawnEffectSkill.Build(data.SkillOnThrow) : null;
            }

            return new ItemMemento
            {
                Id = UniqueIdGenerator.Generate<IItem>().Value,
                Name = data.Name,
                IconName = data.Icon.name,
                UpgradeCount = 0,
                State = state,
                Price = data.Price,
                SkillOnUse = new(skillOnUse),
                SkillOnThrow = new(skillOnThrow),
                HasSameEffect = data.IsSameEffect,
                HasSameSkill = data.IsSameSkill,
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
            var isSuccess = await SkillOnUse.SelectOrDefault(
                skill => skill.Match(
                    spawnEffectSkill => spawnEffectSkill.Use(actor, position, direction, world),
                    itemTargetSkill => itemTargetSkill.Use(actor, this)
                ),
                UniTask.FromResult(false)
            );
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
            var isSuccess = await SkillOnThrow.SelectOrDefault(
                skill => skill.Match(
                    spawnEffectSkill => spawnEffectSkill.Use(actor, position, direction, world),
                    itemTargetSkill => itemTargetSkill.Use(actor, this)
                ),
                UniTask.FromResult(false)
            );
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

        public void Repair()
        {
            _remainingUsages.Value = _maxUsages;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public Dictionary<UpgradePath, System.Action> _GetUpgrades()
        {
            var upgrades = new Dictionary<UpgradePath, System.Action>();
            if (_maxUsages > 1)
            {
                upgrades.Add(
                    new UpgradePath("MaxUsages"),
                    () => _maxUsages += 1
                );
            }
            if (SkillOnUse.HasValue)
            {
                var skillUpgrades = SkillOnUse.Expect("SkillOnUse is null")._GetUpgrades();
                skillUpgrades.ForEach(upgrade => upgrade.Key.Prepend("SkillOnUse"));
                foreach (var upgrade in skillUpgrades)
                {
                    upgrades.Add(upgrade.Key, upgrade.Value);
                }
            }
            if (SkillOnThrow.HasValue)
            {
                var skillUpgrades = SkillOnThrow.Expect("SkillOnThrow is null")._GetUpgrades();
                skillUpgrades.ForEach(upgrade => upgrade.Key.Prepend("SkillOnThrow"));
                foreach (var upgrade in skillUpgrades)
                {
                    if (_hasSameEffect && upgrade.Key.Contains("Effect"))
                    {
                        continue;
                    }
                    upgrades.Add(upgrade.Key, upgrade.Value);
                }
            }
            return upgrades;
        }

        public bool CanUpgrade() => _GetUpgrades().Any();

        public void Upgrade()
        {
            var upgrade = _GetUpgrades().GetAtRandom().Value;
            upgrade();
            _upgradeCount += 1;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public string Info()
        {
            var info = $"{State.GetDescription()}{Name}\n価格: {Price}\n";
            if (_usable)
            {
                if (_hasSameSkill)
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
    }
}