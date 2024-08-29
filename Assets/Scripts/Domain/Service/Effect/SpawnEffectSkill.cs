#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
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
        private readonly string? _log;

        public SpawnEffectSkill(IEffectPosition position, IArea area, IEffect effect, int rushDistance, string? log)
        {
            _position = position;
            _area = area;
            _effect = effect;
            RushDistance = rushDistance;
            _log = log;
        }
        public SpawnEffectSkill(SpawnEffectSkillMemento data) : this(data.Position, data.Area, data.Effect, data.RushDistance, data.Log)
        {
        }

        public SpawnEffectSkill CreateSkillWithEffect(SpawnEffectSkillMemento data)
        {
            return new SpawnEffectSkill(
                data.Position,
                data.Area,
                _effect,
                data.RushDistance,
                data.Log
            );
        }

        public Color Color => _effect.Color;

        public SpawnEffectSkillMemento Serialize()
        {
            return new SpawnEffectSkillMemento
            {
                Position = _position,
                Area = _area,
                Effect = _effect,
                RushDistance = RushDistance,
                Log = _log
            };
        }

        public static SpawnEffectSkillMemento Build(SkillData data)
        {
            return new SpawnEffectSkillMemento
            {
                Position = data.Position,
                Area = data.Area,
                Effect = data.Effect,
                RushDistance = data.RushDistance,
                Log = data.Log
            };
        }

        public static SpawnEffectSkillMemento Build(SkillDataOnUse data)
        {
            return new SpawnEffectSkillMemento
            {
                Position = data.Position,
                Area = data.Area,
                Effect = data.Effect,
                RushDistance = 0,
                Log = ""
            };
        }

        public static SpawnEffectSkillMemento Build(SkillDataOnThrow data)
        {
            return new SpawnEffectSkillMemento
            {
                Position = new AtFeet(),
                Area = data.Area,
                Effect = data.Effect,
                RushDistance = 0,
                Log = ""
            };
        }

        public IEnumerable<Vector2Int> GetArea(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IEffectMap map)
        {
            var spawnPositions = _position.Get(actor, position, direction, map);
            return spawnPositions.SelectMany(spawnPosition => _area.Get(spawnPosition, direction));
        }

        public UniTask<bool> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            if (_log != null && _log != "")
                GameLog.Add($"{actor.GetName(map.Player)}{_log}");
            var spawnPositions = _position.Get(actor, position, direction, map);
            var area = spawnPositions.SelectMany(spawnPosition => _area.Get(spawnPosition, direction));
            map.GetCharactersInArea(area.ToHashSet())
                .ForEach(target =>
                {
                    if (_effect.Impact == Impact.Harmful)
                    {
                        var impactValue = _effect.Evaluate(actor, target);
                        target.WasAttackedBy(actor, impactValue);

                        map.GetCharactersCanSeePosition(target.CurrentPosition)
                            .ForEach(character =>
                                character.Affiliation.OnCharacterAttacked(actor.Affiliation, target.Affiliation,
                                    impactValue));
                    }
                    else if (_effect.Impact == Impact.Beneficial)
                    {
                        var impactValue = _effect.Evaluate(actor, target);
                        target.WasHealedBy(actor, impactValue);

                        map.GetCharactersCanSeePosition(target.CurrentPosition)
                            .ForEach(character =>
                                character.Affiliation.OnCharacterHealed(actor.Affiliation, target.Affiliation,
                                    impactValue));
                    }

                    _effect.Apply(actor, target, map);
                });
            _effect.Apply(actor, area, map);
            return UniTask.FromResult(true);
        }

        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            for (int i = 0; i < RushDistance; i++)
            {
                if (actor.CanMove(position, direction, map))
                    position += direction.Vector();
            }
            var spawnPositions = _position.Get(actor, position, direction, map);
            var area = spawnPositions.SelectMany(spawnPosition => _area.Get(spawnPosition, direction));
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

            return totalEvaluation;
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

        public string Info()
        {
            var info = $"効果: {_effect.Info()}\n発動位置: {_position.Info()}\n範囲: {_area.Info()}";
            if (RushDistance > 0)
                info += $"\n突進距離: {RushDistance}";
            return info;
        }
    }
}