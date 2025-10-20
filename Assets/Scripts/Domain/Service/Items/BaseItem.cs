#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Effect.Position;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
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
    public abstract class BaseItem : IItem, IDisposable
    {
        public Id<IItem> Id { get; private set; }
        public string BaseName { get; private set; }
        public Option<string> CustomName { get; private set; }
        public Sprite Icon { get; private set; }
        public bool IsShiny { get; private set; }
        public int _additionalPrice { get; private set; }
        public float _multiplyPrice { get; private set; }
        public ItemState State { get; private set; }
        public int UpgradeCount { get; private protected set; }
        public int MaxUsages { get; private set; }
        private protected ReactiveProperty<int> _remainingUsages;
        public float UsageLossChance { get; private set; }
        public bool IsCursed { get; private set; }
        public bool IsCurseIdentified { get; private set; }
        public int UpgradeLimit { get; private set; }
        private protected List<IConditionData> _conditions;
        private protected Subject<Unit> _onItemUpdated = new();
        private protected Subject<bool> _onCursedChanged = new();
        private protected Subject<Unit> _onMimicRevealed = new();
        private CompositeDisposable _disposables = new();

        public abstract ItemCategory Category { get; }
        public abstract string RevealedName { get; }
        protected abstract bool HasSameEffect { get; }
        protected abstract bool HasSameSkill { get; }
        public abstract bool UseOnDeath { get; }
        public abstract bool CannotUseIfCursed { get; }
        public abstract bool RequiresLiteracy { get; }
        public abstract bool CannotDropIfCursed { get; }
        public abstract bool IdentifyIfGot { get; }
        public abstract bool IdentifyIfUsed { get; }
        public abstract bool AutoDestroyWhenDisabled { get; }
        public abstract Option<ISkill> SkillOnUse { get; }
        public abstract Option<ISkill> SkillOnThrow { get; }
        private Option<EnemyData> _mimic { get; init; }

        public string DebugName => _fullName;
        private string _fullName => CustomName.UnwrapOr(RevealedName) + _upgradeText();
        private string _upgradeText()
        {
            if (UpgradeCount == 0)
                return "";
            else if (UpgradeCount > 0)
                return $" +{UpgradeCount}";
            else
                return $" {UpgradeCount}";
        }
        public int Price => Mathf.RoundToInt(EvaluatePrice());
        public bool HasActivatableSkillWhenUsed => SkillOnUse.HasValue;
        public bool HasActivatableSkillWhenThrown => SkillOnThrow.HasValue;
        public bool CanActivateWhenUsed => SkillOnUse.HasValue && !IsDisabled;
        public bool CanActivateWhenThrown => SkillOnThrow.HasValue && !IsDisabled;
        public bool HasActivatableSkill => HasActivatableSkillWhenUsed || HasActivatableSkillWhenThrown;
        public bool CanActivate => CanActivateWhenUsed || CanActivateWhenThrown;
        public bool IsDisabled => (IsCursed && CannotUseIfCursed) || _remainingUsages.CurrentValue <= 0;
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUsages;
        public IReadOnlyList<IConditionData> PassiveConditions => _conditions;
        public Observable<Unit> OnItemUpdated => _onItemUpdated;
        public Observable<bool> OnCursedChanged => _onCursedChanged;
        public Observable<Unit> OnMimicRevealed => _onMimicRevealed;

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

        public BaseItem(BaseItemMemento baseItem)
        {
            Id = baseItem.Id;
            BaseName = baseItem.BaseName;
            CustomName = baseItem.CustomName;
            Icon = baseItem.Icon;
            IsShiny = baseItem.IsShiny;
            _additionalPrice = baseItem.AdditionalPrice;
            _multiplyPrice = baseItem.MultiplyPrice;
            State = baseItem.State;
            UpgradeCount = baseItem.UpgradeCount;
            MaxUsages = baseItem.MaxUsages;
            _remainingUsages = new ReactiveProperty<int>(baseItem.RemainingUsages);
            UsageLossChance = baseItem.UsageLossChance;
            IsCursed = baseItem.IsCursed;
            IsCurseIdentified = baseItem.IsCurseIdentified;
            UpgradeLimit = baseItem.UpgradeLimit;
            _conditions = baseItem.Conditions;
            _mimic = baseItem.Mimic;
        }

        public BaseItemMemento SerializeBase()
        {
            return new BaseItemMemento(
                id: Id,
                baseName: BaseName,
                customName: CustomName,
                icon: Icon,
                isShiny: IsShiny,
                additionalPrice: _additionalPrice,
                multiplyPrice: _multiplyPrice,
                state: State,
                upgradeCount: UpgradeCount,
                maxUsages: MaxUsages,
                remainingUsages: _remainingUsages.CurrentValue,
                usageLossChance: UsageLossChance,
                isCursed: IsCursed,
                isCurseIdentified: IsCurseIdentified,
                upgradeLimit: UpgradeLimit,
                conditions: _conditions,
                mimic: _mimic);
        }

        public static BaseItemMemento BuildBase(
            string baseName,
            Sprite icon,
            bool isShiny,
            int additionalPrice,
            float multiplyPrice,
            ItemState state,
            int maxUsages,
            float usageLossChance,
            bool isCursed,
            int upgradeLimit,
            List<IConditionData> conditions,
            Option<EnemyData> mimic
        )
        {
            return new BaseItemMemento(
                id: Id<IItem>.Generate(),
                baseName: baseName,
                customName: Option<string>.None,
                icon: icon,
                isShiny: isShiny,
                additionalPrice: additionalPrice,
                multiplyPrice: multiplyPrice,
                state: state,
                upgradeCount: 0,
                maxUsages: maxUsages,
                remainingUsages: maxUsages,
                usageLossChance: usageLossChance,
                isCursed: isCursed,
                isCurseIdentified: false,
                upgradeLimit: upgradeLimit,
                conditions: conditions,
                mimic: mimic);
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

        private bool ShouldDecreaseUsage(IActorOfEffect actor)
        {
            if (Category == ItemCategory.Books
            && actor.Status.IsFlagStat(FlagStatType.BookMaster)
            && RandUtils.IsGreaterThanProbability(CommonSenseParameters.BookMasterUsageLossChance))
                return false;
            if (Category == ItemCategory.Wands
            && actor.Status.IsFlagStat(FlagStatType.WandMaster)
            && RandUtils.IsGreaterThanProbability(CommonSenseParameters.WandMasterUsageLossChance))
                return false;
            return RandUtils.IsLessThanProbability(UsageLossChance);
        }

        public bool ShouldRevealMimic(IActorOfEffect actor, Vector2Int position, IMap map)
        {
            Debug.Log($"ShouldRevealMimic: {_mimic.IsSome()}");
            if (_mimic.IsSome(out var mimic))
            {
                GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}はモンスターだった");
                map.SpawnEnemyIgnoreMimic(mimic, position, doActImmediately: true, isSlept: false, isShiny: false);
                _onMimicRevealed.OnNext(Unit.Default);
                return true;
            }
            return false;
        }

        public async UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            Debug.Log($"Use:");
            if (ShouldRevealMimic(actor, position, map))
            {
                return SpawnEffectSkillResult.Failed;
            }
            SetCurseIdentified(true);
            if (IsCursed && CannotUseIfCursed)
            {
                GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は呪われているため使用できない");
                return SpawnEffectSkillResult.Failed;
            }
            if (!actor.CanReadItem && RequiresLiteracy)
            {
                GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は文字が読めない");
                return SpawnEffectSkillResult.Failed;
            }

            var result = await SkillOnUse.Expect("SkillOnUse is null").Match(
                spawnEffectSkill => spawnEffectSkill.Use(actor, position, direction, map),
                itemTargetSkill => itemTargetSkill.Use(map.Player, this, actor, map),
                inventoryTargetSkill => inventoryTargetSkill.Use(actor.Inventory, actor, map)
            );
            if (result.Result != SkillResult.Cancelled)
            {
                if (ShouldDecreaseUsage(actor))
                {
                    _remainingUsages.Value -= 1;
                }
                else
                {
                    GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は消費しなかった");
                }
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
            if (ShouldRevealMimic(actor, position, map))
            {
                return SpawnEffectSkillResult.Failed;
            }
            if (IsCursed && CannotUseIfCursed)
            {
                GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は呪われているため使用できない");
                return SpawnEffectSkillResult.Failed;
            }
            if (!actor.CanReadItem && RequiresLiteracy)
            {
                GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は文字が読めない");
                return SpawnEffectSkillResult.Failed;
            }

            var result = await SkillOnThrow.Expect("SkillOnThrow is null").Match(
                spawnEffectSkill => spawnEffectSkill.Use(actor, position, direction, map),
                itemTargetSkill => throw new Exception("The item is not configured to activate this type of skill when thrown."),
                inventoryTargetSkill => throw new Exception("The item is not configured to activate this type of skill when thrown.")
            );
            if (result.Result != SkillResult.Cancelled)
            {
                if (ShouldDecreaseUsage(actor))
                {
                    _remainingUsages.Value -= 1;
                }
                else
                {
                    GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は消費しなかった");
                }
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
            if (UseOnDeath)
            {
                return 0;
            }

            return SkillOnUse.MapOr(
                0,
                skill => skill.Match(
                    spawnEffectSkill => spawnEffectSkill.Evaluate(actor, position, direction, map),
                    itemTargetSkill => itemTargetSkill.Evaluate(),
                    inventoryTargetSkill => inventoryTargetSkill.Evaluate()
                )
            );
        }

        public float EvaluateWhenThrown(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map)
        {
            return SkillOnThrow.MapOr(
                0,
                skill => skill.Match(
                    spawnEffectSkill => spawnEffectSkill.Evaluate(actor, position, direction, map),
                    itemTargetSkill => itemTargetSkill.Evaluate(),
                    inventoryTargetSkill => inventoryTargetSkill.Evaluate()
                )
            );
        }

        public float EvaluateBasePrice()
        {
            var priceOnUse = SkillOnUse.MapOr(0, skill => skill.EvaluatePrice()) * (UseOnDeath ? 5 : 1);
            var priceOnThrow = SkillOnThrow.MapOr(0, skill => skill.EvaluatePrice()) *
                               new ProjectileImpact().EvaluateHitProbability();
            var price = Mathf.Max(priceOnUse, priceOnThrow) * MaxUsages;
            price += _additionalPrice;
            price += _conditions.Sum(condition => condition.EvaluatePrice()) * 100;
            if (IsCursed)
            {
                price *= 0.8f;
            }

            price *= _multiplyPrice;

            return price;
        }

        public float EvaluatePrice()
        {
            var priceOnUse = SkillOnUse.MapOr(0, skill => skill.EvaluatePrice()) * (UseOnDeath ? 5 : 1);
            var priceOnThrow = SkillOnThrow.MapOr(0, skill => skill.EvaluatePrice()) *
                               new ProjectileImpact().EvaluateHitProbability();
            var price = Mathf.Max(priceOnUse, priceOnThrow) * (_remainingUsages.CurrentValue + MaxUsages) / 2 * Mathf.Max(UsageLossChance, 0.1f);
            price += _additionalPrice;
            price += _conditions.Sum(condition => condition.EvaluatePrice()) * 100;
            if (IsCursed)
            {
                price *= 0.8f;
            }

            price *= _multiplyPrice;

            return price;
        }

        public void Repair(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            GameLog.Add(itemHolder.IsVisible, $"{GetName(player, itemPlaceholders)}は修理された");
            _remainingUsages.Value = MaxUsages;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public void SetCursed(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool isCursed)
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
                GameLog.Add(itemHolder.IsVisible, $"{GetName(player, itemPlaceholders)}は呪われた");
            }
            else
            {
                GameLog.Add(itemHolder.IsVisible, $"{GetName(player, itemPlaceholders)}の呪いは解かれた");
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

        public abstract bool CanUpgrade();
        public abstract bool CanDowngrade();
        public abstract void Upgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders);
        public abstract void Downgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders);

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

        protected abstract string FullInfoImpl();

        public string FullInfo()
        {
            var info = $"{State.GetDescription()}{_fullName}";
            if (MaxUsages > 1)
            {
                info += $" ({_remainingUsages.CurrentValue}/{MaxUsages})";
            }
            info += "\n";
            info += $"{Price}Gの価値がある\n";
            info += CursedInfo();
            if (HasActivatableSkill)
            {
                if (HasSameSkill)
                {
                    info += "\n使用または投擲したときの効果...\n" + SkillOnUse.Expect("SkillOnUse is null").Match(
                        spawnEffectSkill => spawnEffectSkill.InfoOnUse(true) + "\n",
                        itemTargetSkill => throw new Exception("SkillOnUse can not be ItemTargetSkill"),
                        inventoryTargetSkill => throw new Exception("SkillOnUse can not be InventoryTargetSkill")
                    );
                    var skillOnUseSuccessProbability = SkillOnUse.Expect("SkillOnUse is null").Match(
                        spawnEffectSkill => spawnEffectSkill.ProbabilityOfSuccess,
                        itemTargetSkill => throw new Exception("SkillOnUse can not be ItemTargetSkill"),
                        inventoryTargetSkill => throw new Exception("SkillOnUse can not be InventoryTargetSkill")
                    );
                    var skillOnThrowSuccessProbability = SkillOnThrow.Expect("SkillOnThrow is null").Match(
                        spawnEffectSkill => spawnEffectSkill.ProbabilityOfSuccess,
                        itemTargetSkill => throw new Exception("SkillOnThrow can not be ItemTargetSkill"),
                        inventoryTargetSkill => throw new Exception("SkillOnThrow can not be InventoryTargetSkill")
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
                            itemTargetSkill => itemTargetSkill.Info(),
                            inventoryTargetSkill => inventoryTargetSkill.Info()
                        ));

                    info += SkillOnThrow.MapOr(
                        "",
                        skill => "\n投擲したときの効果...\n" + skill.Match(
                            spawnEffectSkill => spawnEffectSkill.InfoOnThrow(HasSameEffect),
                            itemTargetSkill => throw new Exception("SkillOnThrow can not be ItemTargetSkill"),
                            inventoryTargetSkill => throw new Exception("SkillOnThrow can not be InventoryTargetSkill")
                        ));
                }
            }

            info += "\n";

            if (UseOnDeath)
            {
                info += "それは死亡時に自動的に使用される\n";
            }

            if (UsageLossChance == 0)
            {
                info += "それは使用可能回数が減少しない\n";
            }
            else if (UsageLossChance < 1)
            {
                info += $"それは{(1 - UsageLossChance):P0}の確率で使用可能回数が減少しない\n";
            }

            foreach (var condition in PassiveConditions)
            {
                info += $"それは{condition.Name}の効果を授ける\n";
            }

            info += FullInfoImpl();

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