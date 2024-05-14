using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data;
using Data.Area;
using Model.Action;
using Sirenix.Utilities;
using UnityEngine;
using Utilities;

namespace Model.Characters.Effect
{
    public class Skill
    {
        private readonly IArea _area;
        public readonly int Power;

        public Skill(SkillData data)
        {
            Power = data.Power;
            _area = data.Area;
        }

        public IEnumerable<Vector2Int> GetArea(Vector2Int position, Direction8 direction)
        {
            return _area.Get(position, direction);
        }

        public UniTask Use(IActor actor, Vector2Int position, Direction8 direction)
        {
            var area = GetArea(position, direction);
            Globals.World.GetCharactersInArea(area.ToHashSet())
                .ForEach(character => character.LoseHp(Formula.Calc(actor, Power)));
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction)
        {
            var area = GetArea(position, direction);
            var characters = Globals.World.GetCharactersInArea(area.ToHashSet());
            if (characters.Any())
                return characters.Sum(character =>
                    (float)Formula.Calc(actor, Power) / character.Stats.MaxHp.CurrentValue);
            return -1;
        }
    }
}