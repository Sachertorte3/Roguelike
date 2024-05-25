using Cysharp.Threading.Tasks;
using Data;
using Data.Area;
using Model.Domain;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Model.Effect
{
    public class Skill
    {
        private readonly IArea _area;
        private readonly IEffect _effect;

        public Skill(SkillData data)
        {
            _area = data.Area;
            _effect = data.Effect;
        }

        public IEnumerable<Vector2Int> GetArea(Vector2Int position, Direction8 direction)
        {
            return _area.Get(position, direction);
        }
        public UniTask Use(IActorOfEffect actor, Vector2Int position, Direction8 direction, IWorld world)
        {
            var area = _area.Get(position, direction);
            world.GetCharactersInArea(area.ToHashSet())
                .ForEach(target =>
                {
                    if (_effect.IsHarmful)
                    {
                        target.WasAttackedBy(actor);
                    }
                    _effect.Apply(actor, target.StatusManager);
                });
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, Vector2Int position, Direction8 direction, IWorld world)
        {
            var area = _area.Get(position, direction);
            var characters = world.GetCharactersInArea(area.ToHashSet());
            if (characters.Any())
                return characters.Sum(target =>
                    _effect.Evaluate(actor, target.StatusManager));
            return -1;
        }
    }
}