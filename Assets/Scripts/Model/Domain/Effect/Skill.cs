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
            var characters = world.Characters;
            world.GetCharactersInArea(area.ToHashSet())
                .ForEach(target =>
                {
                    if (_effect.Impact == Impact.Harmful)
                    {
                        target.WasAttackedBy(actor);

                        characters
                            .Where(character => character.IsVisible(target.CurrentPosition))
                            .ForEach(character => character.Affiliation.OnCharacterAttacked(actor.Affiliation, target.Affiliation));
                    }
                    else if (_effect.Impact == Impact.Beneficial)
                    {
                        target.WasHealedBy(actor);

                        characters
                            .Where(character => character.IsVisible(target.CurrentPosition))
                            .ForEach(character => character.Affiliation.OnCharacterHealed(actor.Affiliation, target.Affiliation));
                    }

                    _effect.Apply(actor, target.StatusManager);
                });
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap world)
        {
            var area = _area.Get(position, direction);
            var characters = world.GetCharactersInArea(area.ToHashSet());
            float totalEvaluation = 0;

            if (characters.Count <= 0)
            {
                return -1;
            }

            foreach (var target in characters)
            {
                // Enemy attacked or ally healed, add to evaluation
                if ((_effect.Impact == Impact.Harmful && actor.Affiliation.IsEnemy(target.Affiliation)) ||
                    (_effect.Impact == Impact.Beneficial && actor.Affiliation.IsAlly(target.Affiliation)))
                {
                    totalEvaluation += _effect.Evaluate(actor, target.StatusManager);
                }
                // Enemy healed or ally attacked, subtract from evaluation
                else if ((_effect.Impact == Impact.Beneficial && actor.Affiliation.IsEnemy(target.Affiliation)) ||
                         (_effect.Impact == Impact.Harmful && actor.Affiliation.IsAlly(target.Affiliation)))
                {
                    totalEvaluation -= _effect.Evaluate(actor, target.StatusManager);
                }
            }

            return totalEvaluation;
        }
    }
}