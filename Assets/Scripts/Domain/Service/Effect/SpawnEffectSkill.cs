#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    public class SpawnEffectSkill : ISerializable<SpawnEffectSkillMemento>, ISkill
    {
        private readonly IArea _area;
        private readonly IEffect _effect;
        private readonly IEffectPosition _position;
        public int RushDistance { get; private set; }
        public float ProbabilityOfSuccess { get; private set; }
        private readonly string? _log;

        public SpawnEffectSkill(IEffectPosition position, IArea area, IEffect effect, int rushDistance, float probabilityOfSuccess, string? log)
        {
            _position = position;
            _area = area;
            _effect = effect;
            RushDistance = rushDistance;
            ProbabilityOfSuccess = probabilityOfSuccess;
            _log = log;
        }
        public SpawnEffectSkill(SpawnEffectSkillMemento data) : this(data.Position, data.Area, data.Effect, data.RushDistance, data.ProbabilityOfSuccess, data.Log)
        {
        }

        public SpawnEffectSkill CreateSkillWithEffect(SpawnEffectSkillMemento data)
        {
            return new SpawnEffectSkill(
                data.Position,
                data.Area,
                _effect,
                data.RushDistance,
                data.ProbabilityOfSuccess,
                data.Log
            );
        }

        public Color Color => _effect.Color;

        public SpawnEffectSkillMemento Serialize()
        {
            return new SpawnEffectSkillMemento
            (
                position: _position,
                area: _area,
                effect: _effect,
                rushDistance: RushDistance,
                probabilityOfSuccess: ProbabilityOfSuccess,
                log: _log
            );
        }

        public static SpawnEffectSkillMemento Build(SkillData data)
        {
            return new SpawnEffectSkillMemento
            (
                position: data.Position,
                area: data.Area,
                effect: data.Effect,
                rushDistance: data.RushDistance,
                probabilityOfSuccess: data.ProbabilityOfSuccess,
                log: data.Log
            );
        }

        public static SpawnEffectSkillMemento Build(SkillDataOnUse data)
        {
            return new SpawnEffectSkillMemento
            (
                position: data.Position,
                area: data.Area,
                effect: data.Effect,
                rushDistance: 0,
                probabilityOfSuccess: data.ProbabilityOfSuccess,
                log: ""
            );
        }

        public static SpawnEffectSkillMemento Build(SkillDataOnThrow data)
        {
            return new SpawnEffectSkillMemento
            (
                position: new AtFeet(),
                area: data.Area,
                effect: data.Effect,
                rushDistance: 0,
                probabilityOfSuccess: data.ProbabilityOfSuccess,
                log: ""
            );
        }

        public IEnumerable<Vector2Int> GetArea(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map, bool onlyVisible=false)
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

        public async UniTask<ISkillResult> Use(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map)
        {
            if (_log != null && _log != "")
                GameLog.Add($"{actor.GetName(map.Player)}{_log}");

            if (Random.value > ProbabilityOfSuccess)
            {
                GameLog.Add($"しかし失敗した！");
                return SpawnEffectSkillResult.Failed;
            }

            if (_position is ProjectileImpact projectileImpact)
            {
                await map.ShowThrowAnimation(projectileImpact.Icon.Value, position, direction, projectileImpact.CanHitLayer.ToArray());
            }
            var area = GetArea(actor, position, direction, map);

            foreach (var target in map.GetEntitiesInArea(area)
                .OrderBy(target => Vector2.Distance(target.CurrentPosition, actor.CurrentPosition))
                .Reverse())
            {
                switch (target)
                {
                    case ICharacter character:
                        if (_effect.Impact == Impact.Harmful)
                        {
                            var impactValue = _effect.Evaluate(actor, character);
                            character.WasAttackedBy(actor, impactValue);

                            map.GetCharactersCanSeePosition(character.CurrentPosition)
                                .Where(target => target != actor && target != actor)
                                .ForEach(c =>
                                    c.Affiliation.OnCharacterAttacked(actor.Affiliation, character.Affiliation,
                                        impactValue));
                        }
                        else if (_effect.Impact == Impact.Beneficial)
                        {
                            var impactValue = _effect.Evaluate(actor, character);
                            character.WasHealedBy(actor, impactValue);

                            map.GetCharactersCanSeePosition(character.CurrentPosition)
                                .Where(target => target != actor && target != actor)
                                .ForEach(c =>
                                    c.Affiliation.OnCharacterHealed(actor.Affiliation, character.Affiliation,
                                        impactValue));
                        }

                        await _effect.Apply(actor, character, map);
                        break;
                    default:
                        await _effect.Apply(actor, target, map);
                        break;
                }
            }
            await _effect.Apply(actor, area, map);
            return SpawnEffectSkillResult.Success(Color, area);
        }

        public float Evaluate(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map)
        {
            for (int i = 0; i < RushDistance; i++)
            {
                if (actor.CanMove(position, direction, map))
                    position += direction.Vector();
            }
            var area = GetArea(actor, position, direction, map, true);
            var characters = map.GetCharactersInArea(area.ToHashSet());
            var (allyImpactRate, neutralImpactRate, enemyImpactRate) = actor.Aggression.GetAggression();
            var totalEvaluation = 0f;

            if (characters.Count <= 0)
            {
                return -1;
            }

            foreach (var target in characters)
            {
                switch (_effect.Impact)
                {
                    case Impact.Harmful:
                        if (actor.IsAlly(target))
                        {
                            totalEvaluation += allyImpactRate * _effect.Evaluate(actor, target);
                        }
                        else if (actor.IsEnemy(target))
                        {
                            totalEvaluation += enemyImpactRate * _effect.Evaluate(actor, target);
                        }
                        else
                        {
                            totalEvaluation += neutralImpactRate * _effect.Evaluate(actor, target);
                        }

                        break;
                    case Impact.Beneficial:
                        if (actor.IsAlly(target))
                        {
                            totalEvaluation += 1 * _effect.Evaluate(actor, target);
                        }
                        else if (actor.IsEnemy(target))
                        {
                            totalEvaluation += -Mathf.Infinity * _effect.Evaluate(actor, target);
                        }
                        else
                        {
                            totalEvaluation += 0 * _effect.Evaluate(actor, target);
                        }

                        break;
                }
            }

            return totalEvaluation * ProbabilityOfSuccess;
        }

        public float EvaluatePrice()
        {
            var price = 0f;
            price += _effect.EvaluatePrice();
            price *= Mathf.Max(_position.EvaluateHitProbability(), RushDistance);
            price *= _area.EvaluateArea();
            return price * ProbabilityOfSuccess;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            var upgrades = new Dictionary<UpgradePath, UpgradeData>();
            foreach (var path in _effect.GenerateUpgradePaths())
            {
                upgrades[UpgradePath.Join("効果", path)] = _effect.GetUpgrades()[path];
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

        public string InfoOnUse()
        {
            var info = $"効果: {_effect.Info()}\n発動位置: {_position.Info()}\n範囲: {_area.Info()}\n発動確率: {ProbabilityOfSuccess:P0}";
            if (RushDistance > 0)
                info += $"\n突進距離: {RushDistance}";
            return info;
        }
        public string InfoOnThrow(bool omitEffects = false)
        {
            var info = "";
            info += $"効果: {(omitEffects ? "使用時と同じ" : _effect.Info())}\n";
            info += $"範囲: {_area.Info()}\n発動確率: {ProbabilityOfSuccess:P0}";
            return info;
        }
    }
}