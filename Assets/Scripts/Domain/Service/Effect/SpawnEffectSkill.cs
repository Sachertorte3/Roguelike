#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Evaluation;
using Domain.Model.Item;
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
        public float ProbabilityOfSuccess { get; private set; }
        private readonly string? _log;

        public SpawnEffectSkill(SpawnEffectSkillMemento data)
        {
            _position = data.Position;
            _area = data.Area;
            _effects = data.Effects;
            Repeats = data.Repeats;
            ProbabilityOfSuccess = data.ProbabilityOfSuccess;
            _log = data.Log;
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
                    foreach (var target in map.Entities.In(area)
                                 .OrderBy(target => Vector2.Distance(target.Entity.CurrentPosition, position))
                                 .Reverse())
                    {
                        switch (target)
                        {
                            case ICharacter character:
                                await effect.Apply(actor, character, position, map);

                                if (effect.Impact == Impact.Harmful)
                                {
                                    var impactValue = effect.Evaluate(actor, character);
                                    character.OnAttackedBy(actor, impactValue);

                                    foreach (var c in map.GetCharactersCanSeePosition(character.Entity.CurrentPosition)
                                                            .Where(target => target != actor && target != actor))
                                    {
                                        c.Affiliation.OnCharacterAttacked(actor.Affiliation, character.Affiliation,
                                            impactValue);
                                    }

                                    if (character.IsDead && effect is not BreakEffect)
                                    {
                                        actor.OnEnemyDefeated(character);
                                    }
                                }
                                else if (effect.Impact == Impact.Beneficial)
                                {
                                    var impactValue = effect.Evaluate(actor, character);
                                    character.OnHealedBy(actor, impactValue);

                                    foreach (var c in map.GetCharactersCanSeePosition(character.Entity.CurrentPosition)
                                                            .Where(target => target != actor && target != actor))
                                    {
                                        c.Affiliation.OnCharacterHealed(actor.Affiliation, character.Affiliation,
                                            impactValue);
                                    }
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

            if (map.Player.Character.VisibleArea.Intersect(area).Any())
            {
                map.SpawnEffect(area, Color);
                await UniTask.Delay(Settings.EffectDisplayTime.CurrentValue);
            }

            return SpawnEffectSkillResult.Success(Color, area);
        }

        public float Evaluate(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map)
        {
            var area = GetArea(actor, position, direction, map, true);
            var characters = map.Characters.In(area);
            var totalEvaluation = 0f;

            if (characters.Count() <= 0)
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
                            totalEvaluation += actor.Aggression.GetAggression(affiliationType) *
                                               effect.Evaluate(actor, target);
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

            return totalEvaluation * Repeats * ProbabilityOfSuccess;
        }

        public float EvaluatePrice()
        {
            var price = 0f;
            foreach (var effect in _effects)
            {
                price += effect.EvaluatePrice();
            }

            price *= _area.EvaluateArea();
            return price * ProbabilityOfSuccess;
        }

        public List<UpgradeData> GetUpgrades()
        {
            return new List<UpgradeData>();
        }

        public Dictionary<string, IHasUpgrades> GetChildren()
        {
            var children = new Dictionary<string, IHasUpgrades>();
            foreach (var effect in _effects)
            {
                children.Add(effect.UpgradePathName, effect);
            }

            children.Add(_position.UpgradePathName, _position);
            children.Add(_area.UpgradePathName, _area);
            return children;
        }

        public string InfoOnUse(bool omitProbabilityOfSuccess = false)
        {
            var info = "";
            if (Repeats > 1)
                info += $"効果は{Repeats}回発動する\n";
            info += $"{_position.Info()}の{_area.Info()}を対象にして\n";
            foreach (var (effect, index) in _effects.Index())
            {
                info += effect.Info();
            }
            if (!omitProbabilityOfSuccess)
                info += $"発動は{ProbabilityOfSuccess:P0}の確率で成功する\n";
            return info;
        }

        public string InfoOnThrow(bool omitEffects = false)
        {
            var info = "";
            if (Repeats > 1)
                info += $"効果は{Repeats}回発動する\n";
            info += $"{_position.Info()}の{_area.Info()}を対象にして\n";
            if (!omitEffects)
            {
                foreach (var (effect, index) in _effects.Index())
                {
                    info += effect.Info();
                }
            }
            else
            {
                info += "使用時と同じ効果を発揮する\n";
            }

            info += $"発動は{ProbabilityOfSuccess:P0}の確率で成功する\n";
            return info;
        }
    }
}