#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Effect.Position;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Service.Effect;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using Unity.Logging;
using Domain.Model.Map;

namespace Domain.Service.Items
{
    public class Item : IItem
    {
        public Id<IItem> Id { get; init; }
        private string _name;
        private readonly List<UpgradePath> _upgradePaths;
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
            _upgradePaths = data.UpgradePaths.Select(path => new UpgradePath(path)).ToList();
            State = data.State;
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

        public string Name => _upgradePaths.Count > 0 ? $"{_name} +{_upgradePaths.Count}" : _name;
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
        public int Price => Mathf.RoundToInt(EvaluatePrice());
        public bool IsDisabled => _remainingUsages.CurrentValue <= 0;
        public int MaxUsages => _maxUsages;
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUsages;
        public IReadOnlyList<IConditionData> PassiveConditions => _conditions;
        public Observable<Unit> OnItemUpdated => _onItemUpdated;

        public ItemMemento Serialize()
        {
            return new ItemMemento
            {
                Id = Id.ToString(),
                Name = _name,
                IconName = Icon.name,
                UpgradePaths = _upgradePaths.Select(path => path.ToString()).ToList(),
                State = State,
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

            var memento = new ItemMemento
            {
                Id = Id<IItem>.Generate().ToString(),
                Name = data.Name,
                IconName = data.Icon.name,
                UpgradePaths = new(),
                State = state,
                SkillOnUse = new(skillOnUse),
                SkillOnThrow = new(skillOnThrow),
                HasSameEffect = data.IsSameEffect,
                HasSameSkill = data.IsSameSkill,
                UseOnDeath = data.UseOnDeath,
                MaxUsages = data.UsageLimit,
                RemainingUsages = data.UsageLimit,
                Conditions = data.PassiveConditions.ToArray()
            };
            var json = JsonUtility.ToJson(memento);
            return JsonUtility.FromJson<ItemMemento>(json);
        }

        public void SetState(ItemState state)
        {
            State = state;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public async UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            var result = await SkillOnUse.Expect("SkillOnUse is null").Match(
                spawnEffectSkill => spawnEffectSkill.Use(actor, position, direction, world),
                itemTargetSkill => itemTargetSkill.Use(actor, this)
            );
            if (result.IsSuccess)
            {
                _remainingUsages.Value -= 1;
                if (State == ItemState.ShopItem)
                {
                    State = ItemState.UsedShopItem;
                }
                _onItemUpdated.OnNext(Unit.Default);
            }
            return result;
        }
        public async UniTask<ISkillResult> UseWhenThrown(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap world)
        {
            var result = await SkillOnThrow.Expect("SkillOnThrow is null").Match(
                spawnEffectSkill => spawnEffectSkill.Use(actor, position, direction, world),
                itemTargetSkill => {
                    Log.Error("The item is not configured to activate this type of skill when thrown.");
                    return itemTargetSkill.Use((IActor)actor, this);
                }
            );
            if (result.IsSuccess)
            {
                _remainingUsages.Value -= 1;
                if (State == ItemState.ShopItem)
                {
                    State = ItemState.UsedShopItem;
                }
                _onItemUpdated.OnNext(Unit.Default);
            }
            return result;
        }

        public float EvaluateWhenUsed(IActor actor, Vector2Int position, Direction8 direction, IMap world)
        {
            if (UseOnDeath && _remainingUsages.CurrentValue <= 1)
            {
                return 0;
            }
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

        public float EvaluatePrice()
        {
            var priceOnUse = SkillOnUse.SelectOrDefault(skill => skill.EvaluatePrice(), 0) * (UseOnDeath ? 5 : 1);
            var priceOnThrow = SkillOnThrow.SelectOrDefault(skill => skill.EvaluatePrice(), 0) * new ProjectileImpact().EvaluateHitProbability();
            var price = Mathf.Max(priceOnUse, priceOnThrow) * _remainingUsages.CurrentValue;
            price += _conditions.Sum(condition => condition.EvaluatePrice()) * 100;
            return price;
        }

        public void Repair()
        {
            _remainingUsages.Value = _maxUsages;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            var upgrades = new Dictionary<UpgradePath, UpgradeData>();
            if (_maxUsages > 1)
            {
                upgrades.Add(
                    new UpgradePath("使用可能回数[小]"),
                    new UpgradeData("使用可能回数[小]", () => { _maxUsages += 3; _remainingUsages.Value += 3; })
                );
                upgrades.Add(
                    new UpgradePath("使用可能回数[大]"),
                    new UpgradeData("使用可能回数[大]", () => { _maxUsages += 5; _remainingUsages.Value += 5; })
                );
            }
            if (SkillOnUse.HasValue)
            {
                var skillUpgrades = SkillOnUse.Expect("SkillOnUse is null").GetUpgrades();
                skillUpgrades.ForEach(upgrade => upgrade.Key.Prepend("使用時"));
                foreach (var upgrade in skillUpgrades)
                {
                    upgrades.Add(upgrade.Key, upgrade.Value);
                }
            }
            if (SkillOnThrow.HasValue)
            {
                var skillUpgrades = SkillOnThrow.Expect("SkillOnThrow is null").GetUpgrades();
                skillUpgrades.ForEach(upgrade => upgrade.Key.Prepend("投擲時"));
                foreach (var upgrade in skillUpgrades)
                {
                    if (_hasSameEffect && upgrade.Key.Contains("効果"))
                    {
                        continue;
                    }
                    upgrades.Add(upgrade.Key, upgrade.Value);
                }
            }
            return upgrades;
        }

        public bool CanUpgrade(string filter = "")
        {
            var upgrades = GetUpgrades();
            if (filter == "")
            {
                return upgrades.Any();
            }
            return upgrades.Any(upgrade => upgrade.Key.Contains(filter));
        }

        public void Upgrade(string filter = "")
        {
            var (path, upgrade) = GetUpgrades().Where(upgrade => upgrade.Key.Contains(filter)).GetAtRandom();
            upgrade.Upgrade();
            _upgradePaths.Add(path);
            _onItemUpdated.OnNext(Unit.Default);
        }

        public string Info()
        {
            var info = $"{State.GetDescription()}{Name}\n価格: {Price}\n";
            if (_usable)
            {
                if (_hasSameSkill)
                {
                    info += $"[使用・投擲時]\n" + SkillOnUse.Expect("SkillOnUse is null").Match(
                        spawnEffectSkill => $"{spawnEffectSkill.InfoOnUse()}\n",
                        itemTargetSkill => $"{itemTargetSkill.Info()}\n"
                    ) + "\n";
                }
                else
                {
                    info += SkillOnUse.SelectOrDefault(skill => $"[使用時]\n" + skill.Match(
                        spawnEffectSkill => spawnEffectSkill.InfoOnUse(),
                        itemTargetSkill => itemTargetSkill.Info()
                    ) + "\n", "");

                    info += SkillOnThrow.SelectOrDefault(skill => skill.Match(
                        spawnEffectSkill => $"[投擲時]\n{spawnEffectSkill.InfoOnThrow(_hasSameEffect)}\n",
                        itemTargetSkill => throw new Exception("SkillOnThrow is not SpawnEffectSkill")
                    ), "");
                }
                info += $"使用可能回数: {_remainingUsages.CurrentValue}/{_maxUsages}\n";
            }

            if (UseOnDeath)
            {
                info += "死亡時に自動的に使用される\n";
            }

            foreach (var path in _upgradePaths)
            {
                info += $"アップグレード: {GetUpgrades()[path].Description}\n";
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
            return Id.Value.GetHashCode();

        }
    }
}