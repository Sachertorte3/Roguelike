using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data;
using Data.Area;
using Data.Character;
using Data.Effect;
using Model.Domain.Characters;
using UnityEngine;
using Utilities;

namespace Model.Domain.Effect
{
    public class Skill : ISerializable<SkillMemento>
    {
        private readonly IArea _area;
        private readonly IEffect _effect;

        public Skill(SkillData data)
        {
            _area = data.Area;
            _effect = data.Effect;
        }

        public Skill(SkillMemento data)
        {
            _area = data.Area;
            _effect = data.Effect;
        }

        public SkillMemento Serialize()
        {
            return new SkillMemento(_area, _effect);
        }

        public IEnumerable<Vector2Int> GetArea(Vector2Int position, Direction8 direction)
        {
            return _area.Get(position, direction);
        }

        public UniTask Use(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap world)
        {
            var area = _area.Get(position, direction);
            world.GetCharactersInArea(area.ToHashSet())
                .ForEach(target =>
                {
                    if (_effect.Impact == Impact.Harmful)
                    {
                        var impactValue = _effect.Evaluate(actor, target);
                        target.WasAttackedBy(actor, impactValue);

                        world.GetCharactersCanSeePosition(target.CurrentPosition)
                            .ForEach(character => character.Affiliation.OnCharacterAttacked(actor.Affiliation, target.Affiliation, impactValue));
                    }
                    else if (_effect.Impact == Impact.Beneficial)
                    {
                        var impactValue = _effect.Evaluate(actor, target);
                        target.WasHealedBy(actor, impactValue);

                        world.GetCharactersCanSeePosition(target.CurrentPosition)
                            .ForEach(character => character.Affiliation.OnCharacterHealed(actor.Affiliation, target.Affiliation, impactValue));
                    }

                    _effect.Apply(actor, target, world);
                });
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap world)
        {
            var area = _area.Get(position, direction);
            var characters = world.GetCharactersInArea(area.ToHashSet());
            var (allyImpactRate, neutralImpactRate, enemyImpactRate) = actor.Aggression.GetAggression();
            float totalEvaluation = -0.01f;//効果がないなら使わない

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
    }
}

