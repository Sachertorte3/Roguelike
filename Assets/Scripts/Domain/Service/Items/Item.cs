#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Condition;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Effect.Position;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Effect;
using Domain.Service.Logs;
using R3;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Domain.Service.Items
{
    public class Item : IItem
    {
        public Id<IItem> Id { get; init; }
        public ItemCategory Category { get; init; }
        public string BaseName { get; init; }
        public string RevealedName { get; init; }
        public string UnknownName(ItemPlaceholders itemPlaceholders) => $"?{itemPlaceholders.GetPlaceholder(BaseName, Category)}?";
        public string DebugName => _fullName;
        private string _fullName => _upgradePaths.Count > 0 ? $"{RevealedName} +{AppliedUpgrades}" : RevealedName;
        private readonly List<UpgradePath> _upgradePaths;
        public int AppliedUpgrades => _upgradePaths.Count;
        private int _maxUsages;
        private readonly ReactiveProperty<int> _remainingUsages;
        private readonly Option<ISkill> _skillOnUse;
        private readonly Option<ISkill> _skillOnThrow;
        private readonly List<IConditionData> _conditions;
        private readonly Subject<Unit> _onItemUpdated = new();
        private readonly Subject<bool> _onCursedChanged = new();
        public Item(ItemData data) : this(Build(data))
        {
        }

        public Item(ItemMemento data)
        {
            Id = new Id<IItem>(data.Id);
            Category = data.Category;
            BaseName = data.BaseName;
            RevealedName = data.Name;
            Icon = Addressables.LoadAssetAsync<Sprite>($"Assets/Images/icons_full_16.png[{data.IconName}]")
                .WaitForCompletion();
            IsShiny = data.IsShiny;
            State = data.State;
            _upgradePaths = data.UpgradePaths.Select(path => new UpgradePath(path)).ToList();
            _skillOnUse = data.SkillOnUse.Map(skill => skill.Deserialize());
            _skillOnThrow = data.SkillOnThrow.Map(skill => skill.Match(
                memento =>
                {
                    if (data.HasSameEffect)
                    {
                        return _skillOnUse.Expect("SkillOnUse is null").Match(
                            spawnEffectSkillOnUse => spawnEffectSkillOnUse.CopyWith(
                                position: memento.Position,
                                area: memento.Area,
                                effect: null,
                                rushDistance: memento.RushDistance,
                                backStepDistance: memento.BackStepDistance,
                                probabilityOfSuccess: memento.ProbabilityOfSuccess,
                                log: memento.Log
                            ),
                            itemTargetSkill => throw new Exception("SkillOnUse is not SpawnEffectSkill")
                        );
                    }

                    if (data.HasSameSkill)
                    {
                        return _skillOnUse.Expect("SkillOnUse is null").Match(
                            spawnEffectSkillOnUse => spawnEffectSkillOnUse.CopyWith(
                                position: null,
                                area: null,
                                effect: null,
                                rushDistance: null,
                                backStepDistance: null,
                                probabilityOfSuccess: memento.ProbabilityOfSuccess,
                                log: null
                            ),
                            itemTargetSkill => throw new Exception("SkillOnUse is not SpawnEffectSkill")
                        );
                    }

                    return new SpawnEffectSkill(memento);
                },
                itemTargetSkillMemento => (ISkill)new ItemTargetSkill(itemTargetSkillMemento)
            ));
            _hasSameEffect = data.HasSameEffect;
            _hasSameSkill = data.HasSameSkill;
            UseOnDeath = data.UseOnDeath;
            _maxUsages = data.MaxUsages;
            _remainingUsages = new ReactiveProperty<int>(data.RemainingUsages);
            IsCursed = data.IsCursed;
            CannotUseIfCursed = data.CannotUseIfCursed;
            CannotDropIfCursed = data.CannotDropIfCursed;
            IdentifyIfGot = data.IdentifyIfGot;
            IdentifyIfUsed = data.IdentifyIfUsed;
            IsCurseIdentified = data.IsCurseIdentified;
            UpgradeLimit = data.UpgradeLimit;
            _conditions = data.Conditions.ToList();
        }

        public string GetName(IHasInventory player, ItemPlaceholders itemPlaceholders)
        {
            if (player.IsKnownItem(this))
                return _fullName;
            return UnknownName(itemPlaceholders);
        }
        public Sprite Icon { get; init; }
        public bool IsShiny { get; init; }
        public ItemState State { get; private set; }
        public bool CanActivateWhenUsed => SkillOnUse.HasValue;
        public bool CanActivateWhenThrown => SkillOnThrow.HasValue;
        public Option<ISkill> SkillOnUse => _skillOnUse;
        public Option<ISkill> SkillOnThrow => _skillOnThrow;
        private readonly bool _hasSameEffect;
        private readonly bool _hasSameSkill;
        private bool _usable => CanActivateWhenUsed || CanActivateWhenThrown;
        public bool UseOnDeath { get; init; }
        public int Price => Mathf.RoundToInt(EvaluatePrice());
        public bool IsDisabled => _remainingUsages.CurrentValue <= 0;
        public int MaxUsages => _maxUsages;
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUsages;
        public bool IsCursed { get; private set; }
        public bool CannotUseIfCursed { get; init; }
        public bool CannotDropIfCursed { get; init; }
        public bool IdentifyIfGot { get; init; }
        public bool IdentifyIfUsed { get; init; }
        public bool IsCurseIdentified { get; private set; }
        public int UpgradeLimit { get; init; }
        public IReadOnlyList<IConditionData> PassiveConditions => _conditions;
        public Observable<Unit> OnItemUpdated => _onItemUpdated;
        public Observable<bool> OnCursedChanged => _onCursedChanged;
        public ItemMemento Serialize()
        {
            return new ItemMemento
            (
                Id.ToString(),
                Category,
                BaseName,
                RevealedName,
                Icon.name,
                IsShiny,
                upgradePaths: _upgradePaths.Select(path => path.ToString()).ToList(),
                state: State,
                skillOnUse: _skillOnUse.Map(skill => skill.Serialize()),
                skillOnThrow: _skillOnThrow.Map(skill => skill.Serialize()),
                hasSameEffect: _hasSameEffect,
                hasSameSkill: _hasSameSkill,
                useOnDeath: UseOnDeath,
                maxUsages: _maxUsages,
                remainingUsages: _remainingUsages.CurrentValue,
                isCursed: IsCursed,
                cannotUseIfCursed: CannotUseIfCursed,
                cannotDropIfCursed: CannotDropIfCursed,
                identifyIfGot: IdentifyIfGot,
                identifyIfUsed: IdentifyIfUsed,
                isCurseIdentified: IsCurseIdentified,
                upgradeLimit: UpgradeLimit,
                conditions: _conditions.ToArray()
            );
        }

        public static ItemMemento Build(ItemData data, ItemState state = ItemState.None)
        {
            var skillOnUse = data.EffectType switch
            {
                ItemEffectType.SpawnEffect => data.SpawnEffectsOnUse
                    ? (ISkillMemento)SpawnEffectSkill.Build(data.SkillOnUse)
                    : null,
                ItemEffectType.ItemTarget => new ItemTargetSkill(ItemTargetSkill.Build(data.ItemEffect)).Serialize(),
                _ => null
            };
            var skillOnThrow = data.SpawnEffectsOnThrow
                    ? (ISkillMemento)SpawnEffectSkill.Build(data.SkillOnThrow)
                    : null;

            var memento = new ItemMemento
            (
                Id<IItem>.Generate().ToString(),
                data.Category,
                data.name,
                data.name,
                data.Icon.name,
                data.IsShiny,
                upgradePaths: new List<string>(),
                state: state,
                skillOnUse: skillOnUse.ToOption(),
                skillOnThrow: skillOnThrow.ToOption(),
                hasSameEffect: data.IsSameEffect,
                hasSameSkill: data.IsSameSkill,
                useOnDeath: data.UseOnDeath,
                maxUsages: data.UsageLimit,
                remainingUsages: data.UsageLimit,
                isCursed: false,
                cannotUseIfCursed: data.CannotUseIfCursed,
                cannotDropIfCursed: data.CannotDropIfCursed,
                identifyIfGot: data.IdentifyIfGot,
                identifyIfUsed: data.IdentifyIfUsed,
                isCurseIdentified: false,
                upgradeLimit: data.UpgradeLimit,
                conditions: data.PassiveConditions.ToArray()
            );
            var json = JsonUtility.ToJson(memento);
            return JsonUtility.FromJson<ItemMemento>(json); //MEMO: To break the sharing of references
        }

        public void SetState(ItemState state)
        {
            State = state;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public async UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            if (IsCursed && CannotUseIfCursed)
            {
                GameLog.Add($"{GetName(actor, map.ItemPlaceholders)}は呪われているため使用できない");
                SetCurseIdentified(true);
                return SpawnEffectSkillResult.Failed;
            }

            var result = await SkillOnUse.Expect("SkillOnUse is null").Match(
                spawnEffectSkill => spawnEffectSkill.Use(actor, position, direction, map),
                itemTargetSkill => itemTargetSkill.Use(actor, this, map)
            );
            if (result.Result != SkillResult.Cancelled)
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

        public async UniTask<ISkillResult> UseWhenThrown(IActorOfEffect actor, Vector2Int position,
            Direction8 direction, IMap map)
        {
            if (IsCursed && CannotUseIfCursed)
            {
                return SpawnEffectSkillResult.Failed;
            }

            var result = await SkillOnThrow.Expect("SkillOnThrow is null").Match(
                spawnEffectSkill => spawnEffectSkill.Use(actor, position, direction, map),
                itemTargetSkill =>
                {
                    Log.Error("The item is not configured to activate this type of skill when thrown.");
                    return itemTargetSkill.Use((IActor)actor, this, map);
                }
            );
            if (result.Result != SkillResult.Cancelled)
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

        public float EvaluateWhenUsed(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            if (IsCursed && CannotUseIfCursed)
            {
                return 0;
            }

            if (UseOnDeath && _remainingUsages.CurrentValue <= 1)
            {
                return 0;
            }

            return SkillOnUse.MapOr(
                0,
                skill => skill.Match(
                    spawnEffectSkill => spawnEffectSkill.Evaluate(actor, position, direction, map),
                    itemTargetSkill => itemTargetSkill.Evaluate(actor, this)
                )
            );
        }

        public float EvaluateWhenThrown(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            if (IsCursed && CannotUseIfCursed)
            {
                return 0;
            }

            return SkillOnThrow.MapOr(
                0,
                skill => skill.Match(
                    spawnEffectSkill => spawnEffectSkill.Evaluate(actor, position, direction, map),
                    itemTargetSkill => itemTargetSkill.Evaluate(actor, this)
                )
            );
        }

        public float EvaluateBasePrice()
        {
            var priceOnUse = SkillOnUse.MapOr(0, skill => skill.EvaluatePrice()) * (UseOnDeath ? 5 : 1);
            var priceOnThrow = SkillOnThrow.MapOr(0, skill => skill.EvaluatePrice()) *
                               new ProjectileImpact().EvaluateHitProbability();
            var price = Mathf.Max(priceOnUse, priceOnThrow) * MaxUsages;
            price += _conditions.Sum(condition => condition.EvaluatePrice()) * 100;
            if (IsCursed)
            {
                price *= 0.8f;
            }
            return price;
        }

        public float EvaluatePrice()
        {
            var priceOnUse = SkillOnUse.MapOr(0, skill => skill.EvaluatePrice()) * (UseOnDeath ? 5 : 1);
            var priceOnThrow = SkillOnThrow.MapOr(0, skill => skill.EvaluatePrice()) *
                               new ProjectileImpact().EvaluateHitProbability();
            var price = Mathf.Max(priceOnUse, priceOnThrow) * (_remainingUsages.CurrentValue + MaxUsages) / 2;
            price += _conditions.Sum(condition => condition.EvaluatePrice()) * 100;
            if (IsCursed)
            {
                price *= 0.8f;
            }
            return price;
        }

        public void Repair(IHasInventory player, ItemPlaceholders itemPlaceholders)
        {
            GameLog.Add($"{GetName(player, itemPlaceholders)}は修理された");
            _remainingUsages.Value = _maxUsages;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public void SetCursed(IHasInventory actor, ItemPlaceholders itemPlaceholders, bool isCursed)
        {
            SetCurseIdentified(true);
            if (IsCursed == isCursed)
            {
                _onCursedChanged.OnNext(isCursed);
                return;
            }

            IsCursed = isCursed;
            if (isCursed)
            {
                GameLog.Add($"{GetName(actor, itemPlaceholders)}は呪われた");
            }
            else
            {
                GameLog.Add($"{GetName(actor, itemPlaceholders)}の呪いは解かれた");
            }
            _onCursedChanged.OnNext(isCursed);
            _onItemUpdated.OnNext(Unit.Default);
        }

        public void SetCurseIdentified(bool isCurseIdentified)
        {
            IsCurseIdentified = isCurseIdentified;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            var upgrades = new Dictionary<UpgradePath, UpgradeData>();
            if (_maxUsages > 1)
            {
                upgrades.Add(
                    new UpgradePath("使用可能回数[小]"),
                    new UpgradeData("使用可能回数[小]",
                    () =>
                    {
                        _maxUsages += 3;
                        _remainingUsages.Value += 3;
                    },
                    () =>
                    {
                        _maxUsages -= 3;
                        _remainingUsages.Value = Mathf.Max(1, _remainingUsages.Value - 3);
                    })
                );
                upgrades.Add(
                    new UpgradePath("使用可能回数[大]"),
                    new UpgradeData("使用可能回数[大]",
                    () =>
                    {
                        _maxUsages += 5;
                        _remainingUsages.Value += 5;
                    },
                    () =>
                    {
                        _maxUsages -= 5;
                        _remainingUsages.Value = Mathf.Max(1, _remainingUsages.Value - 5);
                    })
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
            if (_upgradePaths.Count >= UpgradeLimit)
            {
                return false;
            }

            var upgrades = GetUpgrades();
            if (filter == "")
            {
                return upgrades.Any();
            }

            return upgrades.Any(upgrade => upgrade.Key.Contains(filter));
        }

        public void Upgrade(IHasInventory player, ItemPlaceholders itemPlaceholders, string filter = "")
        {
            var (path, upgrade) = GetUpgrades().Where(upgrade => upgrade.Key.Contains(filter)).GetAtRandom();
            if (player.IsKnownItem(this))
            {
                GameLog.Add($"{_fullName}は{upgrade.Description}の効果を得た");
            }
            else
            {
                GameLog.Add($"{GetName(player, itemPlaceholders)}は何かの効果を得た");
            }
            upgrade.Upgrade();
            _upgradePaths.Add(path);
            _onItemUpdated.OnNext(Unit.Default);
        }

        public void Downgrade(IHasInventory player, ItemPlaceholders itemPlaceholders)
        {
            if (_upgradePaths.Count == 0)
            {
                return;
            }

            var path = _upgradePaths.GetAtRandom();
            _upgradePaths.Remove(path);
            var upgrade = GetUpgrades()[path];
            if (player.IsKnownItem(this))
            {
                GameLog.Add($"{_fullName}の{upgrade.Description}は消えた");
            }
            else
            {
                GameLog.Add($"{GetName(player, itemPlaceholders)}の何かの効果は消えた");
            }
            upgrade.Downgrade();
            _onItemUpdated.OnNext(Unit.Default);
        }

        public string Info(IHasInventory player, ItemPlaceholders itemPlaceholders)
        {
            if (player.IsKnownItem(this))
            {
                return FullInfo();
            }
            else
            {
                var info = $"{State.GetDescription()}{UnknownName(itemPlaceholders)}\n";
                if (IsCurseIdentified && IsCursed)
                    info += $"呪われている\n";
                else if (IsCurseIdentified && !IsCursed)
                    info += $"呪われていない\n";
                else
                    info += $"呪い状態不明\n";
                if (CanActivateWhenUsed)
                    info += $"使用可能\n";
                if (CanActivateWhenThrown)
                    info += $"投擲可能\n";
                return info;
            }
        }

        public string DebugInfo() => FullInfo();

        public string FullInfo()
        {
            var info = $"{State.GetDescription()}{_fullName}\n";
            info += $"価格: {Price}\n";
            if (IsCursed)
                info += $"呪われている\n";
            else
                info += $"呪われていない\n";
            if (_usable)
            {
                if (_hasSameSkill)
                {
                    info += "[使用・投擲時]\n" + SkillOnUse.Expect("SkillOnUse is null").Match(
                        spawnEffectSkill => spawnEffectSkill.InfoOnUse(true) + "\n",
                        itemTargetSkill => throw new Exception("SkillOnUse is not SpawnEffectSkill")
                    );
                    var skillOnUseSuccessProbability = SkillOnUse.Expect("SkillOnUse is null").Match(
                        spawnEffectSkill => spawnEffectSkill.ProbabilityOfSuccess,
                        itemTargetSkill => throw new Exception("SkillOnUse is not SpawnEffectSkill")
                    );
                    var skillOnThrowSuccessProbability = SkillOnThrow.Expect("SkillOnThrow is null").Match(
                        spawnEffectSkill => spawnEffectSkill.ProbabilityOfSuccess,
                        itemTargetSkill => throw new Exception("SkillOnThrow is not SpawnEffectSkill")
                    );
                    info += $"発動確率(使用時): {skillOnUseSuccessProbability:P0}\n";
                    info += $"発動確率(投擲時): {skillOnThrowSuccessProbability:P0}\n";
                }
                else
                {
                    info += SkillOnUse.MapOr(
                        "",
                        skill => "[使用時]\n" + skill.Match(
                            spawnEffectSkill => spawnEffectSkill.InfoOnUse(),
                            itemTargetSkill => itemTargetSkill.Info()
                        ) + "\n");

                    info += SkillOnThrow.MapOr(
                        "",
                        skill => $"[投擲時]\n" + skill.Match(
                            spawnEffectSkill => spawnEffectSkill.InfoOnThrow(_hasSameEffect),
                            itemTargetSkill => throw new Exception("SkillOnThrow is not SpawnEffectSkill")
                        ) + "\n");
                }

                info += $"使用可能回数: {_remainingUsages.CurrentValue}/{_maxUsages}\n";
            }

            if (UseOnDeath)
            {
                info += "死亡時に自動的に使用される\n";
            }

            if (_upgradePaths.Any() || CanUpgrade())
            {
                info += $"アップグレード ({_upgradePaths.Count}/{UpgradeLimit})\n";

                foreach (var path in _upgradePaths)
                {
                    info += $"{GetUpgrades()[path].Description}\n";
                }
            }

            foreach (var condition in PassiveConditions)
            {
                info += $"パッシブ効果: {condition.Name}\n";
            }

            return info;
        }

        public bool Equals(IItem other)
        {
            return other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.Value.GetHashCode();
        }
    }
}