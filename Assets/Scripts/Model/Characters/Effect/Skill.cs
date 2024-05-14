using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data;
using Data.Area;
using Model.Action;
using UnityEngine;
using Utilities;

namespace Model.Characters.Effect
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
        public UniTask Use(IActorOfEffect actor, Vector2Int position, Direction8 direction)
        {
            var area = _area.Get(position, direction);
            Globals.World.GetCharactersInArea(area.ToHashSet())
                .ForEach(target => _effect.Apply(actor, target));
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, Vector2Int position, Direction8 direction)
        {
            var area = _area.Get(position, direction);
            var characters = Globals.World.GetCharactersInArea(area.ToHashSet());
            if (characters.Any())
                return characters.Sum(target =>
                    _effect.Evaluate(actor, target));
            return -1;
        }
    }
}