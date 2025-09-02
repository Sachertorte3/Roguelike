#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
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
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public abstract class BaseItem : IItem, IHasUpgrades, IDisposable
    {
        public Id<IItem> Id { get; private set; }
        public ItemCategory Category { get; private set; }
        public string BaseName { get; private set; }
        public abstract string RevealedName { get; }
        public Option<string> CustomName { get; private set; }
        private protected List<UpgradePath> _upgradePaths;
        public int MaxUsages { get; private set; }
        private protected ReactiveProperty<int> _remainingUsages;
        private protected Option<Storage> _itemStorage;
        private protected List<IConditionData> _conditions;
        private Subject<Unit> _onItemUpdated = new();
        private Subject<bool> _onCursedChanged = new();
        private CompositeDisposable _disposables = new();

        public Sprite Icon { get; private set; }
        public bool IsShiny { get; private set; }
        public ItemState State { get; private set; }
        private bool _hasSameEffect;
        private bool _hasSameSkill;
        public bool UseOnDeath { get; private set; }
        public bool IsCursed { get; private set; }
        public bool CannotUseIfCursed { get; private set; }
        public bool CannotDropIfCursed { get; private set; }
        public bool IdentifyIfGot { get; private set; }
        public bool IdentifyIfUsed { get; private set; }
        public bool IsCurseIdentified { get; private set; }
        public bool AutoDestroyWhenDisabled { get; private set; }
        public int UpgradeLimit { get; private set; }

        public string DebugName => _fullName;
        private string _fullName => CustomName.UnwrapOr(RevealedName) + (_upgradePaths.Count > 0 ? $" +{AppliedUpgrades}" : "");
        public int AppliedUpgrades => _upgradePaths.Count;
        public bool HasActivatableSkillWhenUsed => SkillOnUse.HasValue;
        public bool HasActivatableSkillWhenThrown => SkillOnThrow.HasValue;
        public bool CanActivateWhenUsed => SkillOnUse.HasValue && !IsDisabled;
        public bool CanActivateWhenThrown => SkillOnThrow.HasValue && !IsDisabled;
        public abstract Option<ISkill> SkillOnUse { get; }
        public abstract Option<ISkill> SkillOnThrow { get; }
        public bool HasActivatableSkill => HasActivatableSkillWhenUsed || HasActivatableSkillWhenThrown;
        public bool CanActivate => CanActivateWhenUsed || CanActivateWhenThrown;
        public Option<IStorage> ItemStorage => _itemStorage.Map(storage => (IStorage)storage);
        public int Price => Mathf.RoundToInt(EvaluatePrice());
        public bool IsDisabled => _remainingUsages.CurrentValue <= 0;
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUsages;
        public IReadOnlyList<IConditionData> PassiveConditions => _conditions;
        public Observable<Unit> OnItemUpdated => _onItemUpdated;
        public Observable<bool> OnCursedChanged => _onCursedChanged;

        public string UnknownName(ItemPlaceholders itemPlaceholders)
        {
            return $"?{CustomName.UnwrapOr(itemPlaceholders.GetPlaceholder(BaseName, Category))}?";
        }
        public string GetName(IPlayer player, ItemPlaceholders itemPlaceholders)
        {
            if (player.Character.IsKnownItem(this))
                return _fullName;
            return UnknownName(itemPlaceholders);
        }

        public BaseItem(
            Id<IItem> id,
            ItemCategory category,
            string baseName,
            Option<string> customName,
            Sprite icon,
            bool isShiny,
            ItemState state,
            List<string> upgradePaths,
            bool hasSameEffect,
            bool hasSameSkill,
            bool useOnDeath,
            Option<StorageMemento> storage,
            int maxUsages,
            int remainingUsages,
            bool isCursed,
            bool cannotUseIfCursed,
            bool cannotDropIfCursed,
            bool identifyIfGot,
            bool identifyIfUsed,
            bool isCurseIdentified,
            bool autoDestroyWhenDisabled,
            int upgradeLimit,
            List<IConditionData> conditions)
        {
            Id = id;
            Category = category;
            BaseName = baseName;
            CustomName = customName;
            Icon = icon;
            IsShiny = isShiny;
            State = state;
            _upgradePaths = upgradePaths.Select(path => new UpgradePath(path)).ToList();
            _hasSameEffect = hasSameEffect;
            _hasSameSkill = hasSameSkill;
            _itemStorage = storage.Map(storage =>
            {
                var itemStorage = new Storage(storage);
                itemStorage.OnItemChanged.Subscribe(_ =>
                {
                    _onItemUpdated.OnNext(Unit.Default);
                }).AddTo(_disposables);
                itemStorage.OnItemUpdated.Subscribe(_ =>
                {
                    _onItemUpdated.OnNext(Unit.Default);
                }).AddTo(_disposables);
                return itemStorage;
            });
            UseOnDeath = useOnDeath;
            MaxUsages = maxUsages;
            _remainingUsages = new ReactiveProperty<int>(remainingUsages);
            IsCursed = isCursed;
            CannotUseIfCursed = cannotUseIfCursed;
            CannotDropIfCursed = cannotDropIfCursed;
            IdentifyIfGot = identifyIfGot;
            IdentifyIfUsed = identifyIfUsed;
            IsCurseIdentified = isCurseIdentified;
            AutoDestroyWhenDisabled = autoDestroyWhenDisabled;
            UpgradeLimit = upgradeLimit;
            _conditions = conditions.ToList();
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void SetState(ItemState state)
        {
            State = state;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public async UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            SetCurseIdentified(true);
            if (IsCursed && CannotUseIfCursed)
            {
                GameLog.Add($"{GetName(map.Player, map.ItemPlaceholders)}は呪われているため使用できない");
                return SpawnEffectSkillResult.Failed;
            }

            var result = await SkillOnUse.Expect("SkillOnUse is null").Match(
                spawnEffectSkill => spawnEffectSkill.Use(actor, position, direction, map),
                itemTargetSkill => itemTargetSkill.Use(map.Player, this, map)
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
                    throw new Exception("The item is not configured to activate this type of skill when thrown.");
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
                    itemTargetSkill => itemTargetSkill.Evaluate(map.Player, this)
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
                    itemTargetSkill => itemTargetSkill.Evaluate(map.Player, this)
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

        public void Repair(IPlayer player, ItemPlaceholders itemPlaceholders)
        {
            GameLog.Add($"{GetName(player, itemPlaceholders)}は修理された");
            _remainingUsages.Value = MaxUsages;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public void SetCursed(IPlayer player, ItemPlaceholders itemPlaceholders, bool isCursed)
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
                GameLog.Add($"{GetName(player, itemPlaceholders)}は呪われた");
            }
            else
            {
                GameLog.Add($"{GetName(player, itemPlaceholders)}の呪いは解かれた");
            }

            _onCursedChanged.OnNext(isCursed);
            _onItemUpdated.OnNext(Unit.Default);
        }

        public void SetCurseIdentified(bool isCurseIdentified)
        {
            IsCurseIdentified = isCurseIdentified;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public void Rename(string name)
        {
            CustomName = Option.Some(name);
            _onItemUpdated.OnNext(Unit.Default);
        }

        public void RevertToDefaultName()
        {
            CustomName = Option.None<string>();
            _onItemUpdated.OnNext(Unit.Default);
        }

        #region Upgrade

        public List<UpgradeData> GetUpgrades()
        {
            var upgrades = new List<UpgradeData>();
            if (MaxUsages > 1)
            {
                upgrades.Add(
                    new UpgradeData("使用可能回数[小]",
                        () =>
                        {
                            MaxUsages += 3;
                            _remainingUsages.Value += 3;
                        },
                        () =>
                        {
                            MaxUsages -= 3;
                            _remainingUsages.Value = Mathf.Max(1, _remainingUsages.Value - 3);
                        })
                );
                upgrades.Add(
                    new UpgradeData("使用可能回数[大]",
                        () =>
                        {
                            MaxUsages += 5;
                            _remainingUsages.Value += 5;
                        },
                        () =>
                        {
                            MaxUsages -= 5;
                            _remainingUsages.Value = Mathf.Max(1, _remainingUsages.Value - 5);
                        })
                );
            }

            return upgrades;
        }

        public Dictionary<string, IHasUpgrades> GetChildren()
        {
            var children = new Dictionary<string, IHasUpgrades>();
            if (SkillOnUse.HasValue)
            {
                children.Add("使用時", SkillOnUse.Expect("SkillOnUse is null"));
            }

            if (SkillOnThrow.HasValue)
            {
                children.Add("投擲時", SkillOnThrow.Expect("SkillOnThrow is null"));
            }

            return children;
        }

        public bool CanUpgrade(string filter = "")
        {
            if (_upgradePaths.Count >= UpgradeLimit)
            {
                return false;
            }

            var upgrades = this.GetUpgradePathsRecursively();
            if (filter == "")
            {
                return upgrades.Any();
            }

            return upgrades.Any(upgrade => upgrade.Contains(filter));
        }

        public void RandomUpgrade(IPlayer player, ItemPlaceholders itemPlaceholders, string filter = "")
        {
            var path = this.GetUpgradePathsRecursively().Where(upgrade => upgrade.Contains(filter)).GetAtRandom();
            Upgrade(player, itemPlaceholders, path);
        }

        public void Upgrade(IPlayer player, ItemPlaceholders itemPlaceholders, UpgradePath path)
        {
            if (player.Character.IsKnownItem(this))
            {
                GameLog.Add($"{_fullName}は{path.GetUpgradeName()}の効果を得た");
            }
            else
            {
                GameLog.Add($"{GetName(player, itemPlaceholders)}は何かの効果を得た");
            }

            _upgradePaths.Add(path);
            Log.Debug($"Upgrade: {path}");
            this.ApplyUpgrade(path);
            _onItemUpdated.OnNext(Unit.Default);
        }

        public void UpgradeNoLog(UpgradePath path)
        {
            _upgradePaths.Add(path);
            Log.Debug($"Upgrade: {path}");
            this.ApplyUpgrade(path);
            _onItemUpdated.OnNext(Unit.Default);
        }

        public void Downgrade(IPlayer player, ItemPlaceholders itemPlaceholders)
        {
            if (_upgradePaths.Count == 0)
            {
                return;
            }

            var path = _upgradePaths.GetAtRandom();
            if (player.Character.IsKnownItem(this))
            {
                GameLog.Add($"{_fullName}の{path.GetUpgradeName()}は消えた");
            }
            else
            {
                GameLog.Add($"{GetName(player, itemPlaceholders)}の何かの効果は消えた");
            }

            _upgradePaths.Remove(path);
            Log.Debug($"Downgrade: {path}");
            this.ApplyDowngrade(path);
            _onItemUpdated.OnNext(Unit.Default);
        }

        #endregion
        #region Info
        public bool IsInfoIdentified(IPlayer player)
        {
            return player.Character.IsKnownItem(this);
        }

        public string CursedInfo()
        {
            if (IsCurseIdentified)
            {
                if (IsCursed)
                    return "それは呪われている\n";
                return "それは呪われていない\n";
            }

            return "それは呪われているかわからない\n";
        }

        public string Info(IPlayer player, ItemPlaceholders itemPlaceholders)
        {
            if (IsInfoIdentified(player))
            {
                return FullInfo();
            }
            else
            {
                return UnknownInfo(itemPlaceholders);
            }
        }

        public string UnknownInfo(ItemPlaceholders itemPlaceholders)
        {
            var info = $"{State.GetDescription()}{UnknownName(itemPlaceholders)}\n";
            info += CursedInfo();
            if (HasActivatableSkillWhenUsed)
                info += "それは使用可能である\n";
            if (HasActivatableSkillWhenThrown)
                info += "それは投擲可能である\n";
            return info;
        }

        public string DebugInfo()
        {
            return FullInfo();
        }

        public string FullInfo()
        {
            var info = $"{State.GetDescription()}{_fullName} ({_remainingUsages.CurrentValue}/{MaxUsages})\n";
            info += $"{Price}Gの価値がある\n";
            info += CursedInfo();
            if (HasActivatableSkill)
            {
                if (_hasSameSkill)
                {
                    info += "\n使用または投擲したときの効果...\n" + SkillOnUse.Expect("SkillOnUse is null").Match(
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
                    info += $"使用時の発動は{skillOnUseSuccessProbability:P0}の確率で成功する\n";
                    info += $"投擲時の発動は{skillOnThrowSuccessProbability:P0}の確率で成功する\n";
                }
                else
                {
                    info += SkillOnUse.MapOr(
                        "",
                        skill => "\n使用したときの効果...\n" + skill.Match(
                            spawnEffectSkill => spawnEffectSkill.InfoOnUse(),
                            itemTargetSkill => itemTargetSkill.Info()
                        ));

                    info += SkillOnThrow.MapOr(
                        "",
                        skill => "\n投擲したときの効果...\n" + skill.Match(
                            spawnEffectSkill => spawnEffectSkill.InfoOnThrow(_hasSameEffect),
                            itemTargetSkill => throw new Exception("SkillOnThrow is not SpawnEffectSkill")
                        ));
                }
            }

            info += "\n";

            if (UseOnDeath)
            {
                info += "それは死亡時に自動的に使用される\n";
            }

            foreach (var condition in PassiveConditions)
            {
                info += $"それは{condition.Name}の効果を授ける\n";
            }

            if (_upgradePaths.Any() || CanUpgrade())
            {
                info += $"アップグレード ({_upgradePaths.Count}/{UpgradeLimit})\n";

                foreach (var path in _upgradePaths)
                {
                    info += $"{path.GetUpgradeName()}\n";
                }
            }

            return info;
        }
        #endregion

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