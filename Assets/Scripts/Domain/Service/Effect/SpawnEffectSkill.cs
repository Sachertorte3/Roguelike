#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    /// <summary>
    /// スキル / アイテム効果の中核。効果を「発生位置(_position) × 範囲(_area) × 効果リスト(_effects)」の
    /// 組み合わせで表現する。位置（足元・前方の着弾点など）から範囲（円・扇形など）を展開し、
    /// その範囲内の対象へ効果リスト（ダメージ・回復・状態異常・生成・破壊など）を順に適用する。
    /// この合成により、攻撃・回復・移動・武器の付与効果などを少ない部品で組み立てられる。
    /// </summary>
    public class SpawnEffectSkill : ISerializable<SpawnEffectSkillMemento>, ISkill
    {
        private readonly IEffectPosition _position;
        private readonly IArea _area;
        private readonly List<IEffect> _effects;
        public int Repeats { get; private set; }
        public float ProbabilityOfSuccess { get; private set; }
        private readonly string? _log;

        public SpawnEffectSkill(SpawnEffectSkillMemento data)
        {
            _position = data.Position;
            _area = data.Area;
            _effects = data.Effects;
            Repeats = data.Repeats;
            ProbabilityOfSuccess = data.ProbabilityOfSuccess;
            RushDistance = data.RushDistance;
            BackStepDistance = data.BackStepDistance;
            _log = data.Log;
        }

        public Color Color => _effects.First().Color;
        public bool IsDirectional => _area.IsDirectional || _position.IsDirectional;

        public int RushDistance { get; private set; }
        public int BackStepDistance { get; private set; }

        /// <summary>アイテム説明テンプレートなど、実行以外からの参照用。</summary>
        public IEffectPosition EffectPosition => _position;

        /// <summary>アイテム説明テンプレートなど、実行以外からの参照用。</summary>
        public IArea EffectArea => _area;

        /// <summary>アイテム説明テンプレートなど、実行以外からの参照用。</summary>
        public IReadOnlyList<IEffect> EffectList => _effects;

        public SpawnEffectSkillMemento Serialize()
        {
            return new SpawnEffectSkillMemento
            (
                _position,
                _area,
                _effects,
                Repeats,
                ProbabilityOfSuccess,
                RushDistance,
                BackStepDistance,
                _log
            );
        }

        public static SpawnEffectSkillMemento Build(ISkillData data)
        {
            return new SpawnEffectSkillMemento
            (
                data.Position,
                data.Area,
                data.Effects,
                data.Repeats,
                data.ProbabilityOfSuccess,
                data.RushDistance,
                data.BackStepDistance,
                data.Log
            );
        }

        public IEnumerable<Vector2Int> GetArea(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map, bool onlyVisible = false)
        {
            position = AdvanceByRushDistance(actor, position, direction, map);
            return GetAreaIgnoreRush(actor, position, direction, map, onlyVisible);
        }

        // 突進斬りなどを表現するため、RushDistance 分だけ前方へ突進した着地点を返す。
        // 壁や移動不可状態に阻まれた時点で止まる。
        private Vector2Int AdvanceByRushDistance(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map)
        {
            for (var i = 0; i < RushDistance; i++)
            {
                if (actor.CanMove(position, direction, map) && !actor.Status.IsFlagStat(FlagStatType.CannotMove))
                    position += direction.Vector();
                else
                    break;
            }

            return position;
        }

        private IEnumerable<Vector2Int> GetAreaIgnoreRush(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map, bool onlyVisible = false)
        {
            var spawnPositions = _position.Get(actor, position, direction, map);
            if (onlyVisible)
            {
                return spawnPositions
                    .Where(map.Player.Character.VisibleArea.Contains)
                    .SelectMany(spawnPosition => _area.Get(spawnPosition, direction, map))
                    .Where(map.Player.Character.VisibleArea.Contains);
            }

            return spawnPositions
                .SelectMany(spawnPosition => _area.Get(spawnPosition, direction, map));
        }
        public async UniTask<ISkillResult> Use(IActorOfEffect actor, IItem? sourceItem, Vector2Int position,
            Direction8 direction, IMap map)
        {
            if (_log != null && _log != "")
                GameLog.Add(actor.IsVisible, $"{actor.GetName(map.Player)}{_log}");

            var effectiveRepeats = GetEffectiveRepeats(actor, sourceItem);
            var successes = RandUtils.RollSuccesses(effectiveRepeats, ProbabilityOfSuccess);

            for (var i = 0; i < successes; i++)
            {
                if (_position is ProjectileImpact projectileImpact)
                {
                    await map.ShowThrowAnimation(projectileImpact.Icon.Value, position, direction,
                        CommonSenseParameters.ThrowDistance, projectileImpact.IsPiercing, projectileImpact.CanHitLayer.ToArray());
                }

                var area = GetAreaIgnoreRush(actor, position, direction, map);
                if (_effects.Any(effect =>
                        effect is AttackEffect ||
                        effect is AbsorbsEffect ||
                        effect is PercentageDamageEffect ||
                        effect is BreakEffect))
                {
                    map.SetGrasses(area, false);
                }

                if (_effects.Any(effect =>
                        effect is AttackEffect ||
                        effect is AbsorbsEffect ||
                        effect is PercentageDamageEffect))
                {
                    map.RevealMimic(area);
                    map.AttackStatue(area);
                }
                // 各エフェクトを「範囲内の対象（遠い順）」と「範囲そのもの」の両方に適用する。
                foreach (var effect in _effects)
                {
                    foreach (var target in map.Entities.In(area)
                                 .OrderBy(target => Vector2.Distance(target.Entity.CurrentPosition, position))
                                 .Reverse())
                    {
                        await ApplyEffectToTarget(effect, actor, target, position, map);
                    }

                    // 対象個別ではなく範囲に作用する効果（草・罠の生成、地形変化など）
                    await effect.Apply(actor, area, map);
                }

                if (map.Player.Character.VisibleArea.Intersect(area).Any())
                {
                    map.SpawnEffect(area, Color);
                    await UniTask.Delay(Settings.GlobalSettings.EffectDisplayTime.CurrentValue);
                }
            }

            if (successes == 0)
            {
                GameLog.AddAppend(actor.IsVisible, "しかし効果がなかった");
                return SpawnEffectSkillResult.Failed;
            }
            else if (successes < effectiveRepeats)
            {
                GameLog.AddAppend(actor.IsVisible, $"{successes}回成功した");
            }

            return SpawnEffectSkillResult.Success;
        }

        private async UniTask ApplyEffectToTarget(IEffect effect, IActorOfEffect actor, IEntity target,
            Vector2Int position, IMap map)
        {
            switch (target)
            {
                case ICharacter character:
                    await effect.Apply(actor, character, position, map);

                    if (effect.Impact == Impact.Harmful)
                    {
                        var impactValue = effect.Evaluate(actor, character);
                        character.OnAttackedBy(actor, impactValue);
                        ReflectAttackToNearbyAffiliations(map, actor, character, impactValue);
                    }
                    else if (effect.Impact == Impact.Beneficial)
                    {
                        var impactValue = effect.Evaluate(actor, character);
                        character.OnHealedBy(actor, impactValue);
                        ReflectHealToNearbyAffiliations(map, actor, character, impactValue);
                    }

                    break;
                default:
                    await effect.Apply(actor, target, position, map);
                    break;
            }
        }

        private void ReflectAttackToNearbyAffiliations(IMap map, IActorOfEffect actor, ICharacter target,
            float impactValue)
        {
            foreach (var witness in CharactersWhoCanSee(map, target, actor))
                witness.Affiliation.OnCharacterAttacked(actor.Affiliation, target.Affiliation, impactValue);
        }

        private void ReflectHealToNearbyAffiliations(IMap map, IActorOfEffect actor, ICharacter target,
            float impactValue)
        {
            foreach (var witness in CharactersWhoCanSee(map, target, actor))
                witness.Affiliation.OnCharacterHealed(actor.Affiliation, target.Affiliation, impactValue);
        }

        // 副作用のない純粋なクエリのため static。対象を視認できる、行為者以外のキャラを返す。
        private static IEnumerable<ICharacter> CharactersWhoCanSee(IMap map, ICharacter target, IActorOfEffect actor)
            => map.GetCharactersCanSeePosition(target.Entity.CurrentPosition).Where(c => c != actor);

        // 敵AIがこのスキルを「今使う価値があるか」を見積もる。盤面への影響を符号付きの評価値にし、
        // 各行動候補の評価値を比較して最も効果の大きい行動を選ばせる（敵AIの行動評価の一部）。
        public float Evaluate(IActorOfEffect actor, IItem? sourceItem, Vector2Int position, Direction8 direction,
            IMap map)
        {
            position = AdvanceByRushDistance(actor, position, direction, map);

            var area = GetArea(actor, position, direction, map, true);
            var characters = map.Characters.In(area);
            // 対象が必要な効果なのに範囲内にキャラがいなければ、無駄撃ちなので評価を下げる。
            if (_effects.Any(ContainsEntityTargetEffect) && !characters.Any())
                return -1;

            var totalEvaluation = 0f;

            foreach (var effect in _effects)
            {
                foreach (var target in characters)
                {
                    switch (effect.Impact)
                    {
                        case Impact.Harmful:
                            // 攻撃系：相手との敵対度（Aggression）が高いほど価値が上がる。
                            var affiliationType = actor.Affiliation.GetAffiliationType(target.Affiliation);
                            totalEvaluation += actor.Aggression.GetAggression(affiliationType) *
                                               effect.Evaluate(actor, target);
                            break;
                        case Impact.Beneficial:
                            // 回復・支援系：味方には加点。敵に当ててしまう構図は -∞ で確実に避け、
                            // 中立には影響なし（0）として扱う。
                            if (actor.IsAlly(target))
                            {
                                totalEvaluation += 1 * effect.Evaluate(actor, target);
                            }
                            else if (actor.IsEnemy(target))
                            {
                                totalEvaluation += -Mathf.Infinity * effect.Evaluate(actor, target);
                            }
                            else
                            {
                                totalEvaluation += 0 * effect.Evaluate(actor, target);
                            }

                            break;
                    }
                }

                totalEvaluation += effect.Evaluate(actor, area);
            }

            return totalEvaluation * GetEffectiveRepeats(actor, sourceItem) * ProbabilityOfSuccess;
        }

        private static bool ContainsEntityTargetEffect(IEffect effect) =>
            effect switch
            {
                EntityTargetEffect => true,
                ActorlessEntityTargetEffect => true,
                RandomEffect random => random.Effects.Any(ContainsEntityTargetEffect),
                _ => false
            };

        private int GetEffectiveRepeats(IActorOfEffect actor, IItem? sourceItem)
        {
            var repeats = Repeats;
            if (sourceItem?.Category == ItemCategory.Potions
                && actor.Status.IsFlagStat(FlagStatType.PotionMaster))
                repeats += CommonSenseParameters.PotionMasterEffectRepeatBonus;

            return repeats;
        }

        public float EvaluatePrice()
        {
            var price = 0f;
            foreach (var effect in _effects)
            {
                price += effect.EvaluatePrice();
            }
            price *= Repeats;

            price *= _area.EvaluateArea();
            price *= _position.EvaluateHitProbability();
            return price * ProbabilityOfSuccess;
        }

        public string Info()
        {
            return InfoOnUse();
        }

        public string InfoOnUse(bool omitProbabilityOfSuccess = false, bool useOrThrowCombinedTargets = false)
        {
            var info = "";
            if (RushDistance > 0)
                info += $"最初に{ItemDescriptionRichText.RichSpatialCells(RushDistance)}前に進む\n";
            var positionInfo = _position.Info();
            var areaInfo = _area.Info();
            info += EffectTargetDescription.OnUse(positionInfo, areaInfo, useOrThrowCombinedTargets) + "\n";
            foreach (var (effect, index) in _effects.Index())
            {
                info += ItemDescriptionRichText.StyleEffectInfo(effect, effect.Info());
            }
            if (Repeats > 1)
                info += $"効果は{ItemDescriptionRichText.RichMeta(Repeats)}回発動する\n";
            if (!omitProbabilityOfSuccess)
                info += ItemDescriptionRichText.ColorPercentagesInPlainText($"成功率：{ProbabilityOfSuccess:P0}\n");

            if (BackStepDistance > 0)
                info += $"最後に{ItemDescriptionRichText.RichSpatialCells(BackStepDistance)}後ろに下がる\n";
            return info;
        }

        public string InfoOnThrow(bool omitEffects = false)
        {
            var info = "";
            if (RushDistance > 0)
                info += $"最初に{ItemDescriptionRichText.RichSpatialCells(RushDistance)}前に進む\n";
            var positionInfo = _position.Info();
            var areaInfo = _area.Info();
            var targetLine = EffectTargetDescription.OnThrow(positionInfo, areaInfo);
            if (!omitEffects)
            {
                info += targetLine + "\n";
                foreach (var (effect, index) in _effects.Index())
                {
                    info += ItemDescriptionRichText.StyleEffectInfo(effect, effect.Info());
                }
            }
            else
            {
                info += targetLine + "\n";
                info += "使用時と同じ効果を発揮する\n";
            }

            if (Repeats > 1)
                info += $"効果は{ItemDescriptionRichText.RichMeta(Repeats)}回発動する\n";

            info += ItemDescriptionRichText.ColorPercentagesInPlainText($"成功率：{ProbabilityOfSuccess:P0}\n");

            if (BackStepDistance > 0)
                info += $"最後に{ItemDescriptionRichText.RichSpatialCells(BackStepDistance)}後ろに下がる\n";
            return info;
        }
    }
}