#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    public class SpawnEffectSkill : ISerializable<SpawnEffectSkillMemento>, ISkill
    {
        private readonly IEffectPosition _position;
        private readonly IArea _area;
        private readonly List<IEffect> _effects;
        public int Repeats { get; private set; }
        public int RushDistance { get; private set; }
        public int BackStepDistance { get; private set; }
        public float ProbabilityOfSuccess { get; private set; }
        private readonly string? _log;

        public SpawnEffectSkill(IEffectPosition position, IArea area, List<IEffect> effect, int repeats, int rushDistance,
            int backStepDistance, float probabilityOfSuccess, string? log)
        {
            _position = position;
            _area = area;
            _effects = effect;
            Repeats = repeats;
            RushDistance = rushDistance;
            BackStepDistance = backStepDistance;
            ProbabilityOfSuccess = probabilityOfSuccess;
            _log = log;
        }

        public SpawnEffectSkill(SpawnEffectSkillMemento data) : this(data.Position, data.Area, data.Effect, data.Repeats,
            data.RushDistance, data.BackStepDistance, data.ProbabilityOfSuccess, data.Log)
        {
        }

        public SpawnEffectSkill CopyWith(
            IEffectPosition? position = null,
            IArea? area = null,
            List<IEffect>? effect = null,
            int? repeats = null,
            int? rushDistance = null,
            int? backStepDistance = null,
            float? probabilityOfSuccess = null,
            string? log = null)
        {
            return new SpawnEffectSkill(
                position ?? _position,
                area ?? _area,
                effect ?? _effects,
                repeats ?? Repeats,
                rushDistance ?? RushDistance,
                backStepDistance ?? BackStepDistance,
                probabilityOfSuccess ?? ProbabilityOfSuccess,
                log ?? _log
            );
        }

        public Color Color => _effects.First().Color;
        public bool IsDirectional => _area.IsDirectional || _position.IsDirectional;

        public SpawnEffectSkillMemento Serialize()
        {
            return new SpawnEffectSkillMemento
            (
                _position,
                _area,
                _effects,
                Repeats,
                RushDistance,
                BackStepDistance,
                ProbabilityOfSuccess,
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
                data.RushDistance,
                data.BackStepDistance,
                data.ProbabilityOfSuccess,
                data.Log
            );
        }

        public IEnumerable<Vector2Int> GetArea(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map, bool onlyVisible = false)
        {
            var spawnPositions = _position.Get(actor, position, direction, map);
            if (onlyVisible)
            {
                return spawnPositions
                    .Where(actor.VisibleArea.Contains)
                    .SelectMany(spawnPosition => _area.Get(spawnPosition, direction, map))
                    .Where(actor.VisibleArea.Contains);
            }

            return spawnPositions
                .SelectMany(spawnPosition => _area.Get(spawnPosition, direction, map));
        }

        public async UniTask<ISkillResult> Use(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map)
        {
            if (_log != null && _log != "")
                GameLog.Add($"{actor.GetName(map.Player)}{_log}");

            if (Random.value > ProbabilityOfSuccess)
            {
                GameLog.Add("しかし効果がなかった");
                return SpawnEffectSkillResult.Failed;
            }

            if (_position is ProjectileImpact projectileImpact)
            {
                await map.ShowThrowAnimation(projectileImpact.Icon.Value, position, direction,
                    CommonSenseParameters.ThrowDistance, projectileImpact.CanHitLayer.ToArray());
            }

            var area = GetArea(actor, position, direction, map);
            if (_effects.Any(effect =>
                effect is AttackEffect ||
                effect is AbsorbsEffect ||
                effect is PercentageDamageEffect ||
                effect is BreakEffect))
            {
                map.SetGrasses(area, false);
            }

            for (var i = 0; i < Repeats; i++)
            {
                foreach (var effect in _effects)
                {
                    foreach (var target in map.GetEntitiesInArea(area)
                                .OrderBy(target => Vector2.Distance(target.CurrentPosition, position))
                                .Reverse())
                    {
                        switch (target)
                        {
                            case ICharacter character:
                                await effect.Apply(actor, character, position, map);

                                if (effect.Impact == Impact.Harmful)
                                {
                                    var impactValue = effect.Evaluate(actor, character);
                                    character.WasAttackedBy(actor, impactValue);

                                    map.GetCharactersCanSeePosition(character.CurrentPosition)
                                        .Where(target => target != actor && target != actor)
                                        .ForEach(c =>
                                            c.Affiliation.OnCharacterAttacked(actor.Affiliation, character.Affiliation,
                                                impactValue));
                                }
                                else if (effect.Impact == Impact.Beneficial)
                                {
                                    var impactValue = effect.Evaluate(actor, character);
                                    character.WasHealedBy(actor, impactValue);

                                    map.GetCharactersCanSeePosition(character.CurrentPosition)
                                        .Where(target => target != actor && target != actor)
                                        .ForEach(c =>
                                            c.Affiliation.OnCharacterHealed(actor.Affiliation, character.Affiliation,
                                                impactValue));
                                }
                                break;
                            default:
                                await effect.Apply(actor, target, position, map);
                                break;
                        }
                    }

                    await effect.Apply(actor, area, map);
                }
            }

            if (map.VisibleArea.Intersect(area).Any())
            {
                map.SpawnEffect(area, Color);
                await UniTask.Delay(Settings.EffectDisplayTime.CurrentValue);
            }

            return SpawnEffectSkillResult.Success(Color, area);
        }

        public float Evaluate(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map)
        {
            for (var i = 0; i < RushDistance; i++)
            {
                if (actor.CanMove(position, direction, map))
                    position += direction.Vector();
            }

            var area = GetArea(actor, position, direction, map, true);
            var characters = map.GetCharactersInArea(area);
            var totalEvaluation = 0f;

            if (characters.Count <= 0)
            {
                return -1;
            }

            foreach (var effect in _effects)
            {
                foreach (var target in characters)
                {
                    switch (effect.Impact)
                    {
                        case Impact.Harmful:
                            var affiliationType = actor.Affiliation.GetAffiliationType(target.Affiliation);
                            totalEvaluation += actor.Aggression.GetAggression(affiliationType) * effect.Evaluate(actor, target);
                            break;
                        case Impact.Beneficial:
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

            return totalEvaluation * ProbabilityOfSuccess;
        }

        public float EvaluatePrice()
        {
            var price = 0f;
            foreach (var effect in _effects)
            {
                price += effect.EvaluatePrice();
            }
            price *= Mathf.Max(_position.EvaluateHitProbability(), RushDistance);
            price *= _area.EvaluateArea();
            return price * ProbabilityOfSuccess;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            var upgrades = new Dictionary<UpgradePath, UpgradeData>();
            foreach (var effect in _effects)
            {
                foreach (var path in effect.GenerateUpgradePaths())
                {
                    upgrades[UpgradePath.Join("効果", path)] = effect.GetUpgrades()[path];
                }
            }

            foreach (var path in _position.GenerateUpgradePaths())
            {
                upgrades[UpgradePath.Join("発動位置", path)] = _position.GetUpgrades()[path];
            }

            foreach (var path in _area.GenerateUpgradePaths())
            {
                upgrades[UpgradePath.Join("範囲", path)] = _area.GetUpgrades()[path];
            }

            return upgrades;
        }

        public string InfoOnUse(bool omitProbabilityOfSuccess = false)
        {
            var info = "";
            if (Repeats > 1)
                info += $"発動回数: {Repeats}回\n";
            foreach (var (effect, index) in _effects.Index())
            {
                info += $"効果{index + 1}: {effect.Info()}\n";
            }
            info += $"発動位置: {_position.Info()}\n";
            info += $"範囲: {_area.Info()}";
            if (RushDistance > 0)
                info += $"\n突進距離: {RushDistance}";
            if (BackStepDistance > 0)
                info += $"\n後退距離: {BackStepDistance}";
            if (!omitProbabilityOfSuccess)
                info += $"\n発動確率: {ProbabilityOfSuccess:P0}";
            return info;
        }

        public string InfoOnThrow(bool omitEffects = false)
        {
            var info = "";
            if (Repeats > 1)
                info += $"発動回数: {Repeats}回\n";
            if (!omitEffects)
            {
                foreach (var (effect, index) in _effects.Index())
                {
                    info += $"効果{index + 1}: {effect.Info()}\n";
                }
            }
            else
            {
                info += "効果: 使用時と同じ\n";
            }
            info += $"発動位置: {_position.Info()}\n";
            info += $"範囲: {_area.Info()}\n";
            info += $"発動確率: {ProbabilityOfSuccess:P0}";
            return info;
        }
    }
}