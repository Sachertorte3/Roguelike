#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Area;
using Domain.Model.Character;
using Domain.Model.Effect;
using Effect;
using Effect.Position;
using UnityEngine;
using Utilities;
using Domain.Model.Action;
using Domain.Service.Logs;

namespace Domain.Service.Effect
{
    public class Skill : ISkill
    {
        private readonly IArea _area;
        private readonly IEffect _effect;
        private readonly string _info;
        private readonly string? _log;
        private readonly IEffectPosition _position;

        public Skill(SkillData data)
        {
            _position = data.Position;
            _area = data.Area;
            _effect = data.Effect;
            _info = data.Info();
            _log = data.Log;
        }

        public Skill(SkillDataOnUse data)
        {
            _position = data.Position;
            _area = data.Area;
            _effect = data.Effect;
            _info = data.Info();
        }

        public Skill(SkillDataOnThrow data)
        {
            _position = new AtFeet();
            _area = data.Area;
            _effect = data.Effect;
            _info = data.Info();
        }

        public Skill(SkillMemento data)
        {
            _position = data.Position;
            _area = data.Area;
            _effect = data.Effect;
            _info = data.Info;
            _log = data.Log;
        }

        public Color Color => _effect.Color;

        public SkillMemento Serialize()
        {
            return new SkillMemento(_position, _area, _effect, _info, _log);
        }

        public IEnumerable<Vector2Int> GetArea(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IEffectMap map)
        {
            var spawnPositions = _position.Get(actor, position, direction, map);
            return spawnPositions.SelectMany(spawnPosition => _area.Get(spawnPosition, direction));
        }

        public UniTask Use(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map)
        {
            if (_log != null)
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
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap world)
        {
            var area = _area.Get(position, direction);
            var characters = world.GetCharactersInArea(area.ToHashSet());
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

        public string Info()
        {
            return _info;
        }
    }
}