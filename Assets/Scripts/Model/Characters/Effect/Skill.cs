using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Scripts.Data;
using Scripts.Data.Area;
using Scripts.Model.Action;
using Scripts.Utilities;
using Sirenix.Utilities;
using UnityEngine;

namespace Scripts.Model.Characters.Effect
{
    public class Skill
    {
        public readonly int Power;
        private readonly IArea _area;
        public IEnumerable<Vector2Int> GetArea(Vector2Int position, Direction8 direction) => _area.Get(position, direction);
        public Skill(SkillData data)
        {
            Power = data.Power;
            _area = data.Area;
        }
        public UniTask Use(IActor actor, Direction8 direction)
        {
            IEnumerable<Vector2Int> area = GetArea(actor.CurrentPosition, direction);
            Globals.World.GetCharactersInArea(area.ToHashSet()).ForEach(character => character.LoseHp(Formula.Calc(actor, Power)));
            return UniTask.CompletedTask;
        }
        public float Evaluate(IActor actor, Direction8 direction)
        {
            IEnumerable<Vector2Int> area = GetArea(actor.CurrentPosition, direction);
            HashSet<Character> characters = Globals.World.GetCharactersInArea(area.ToHashSet());
            if (characters.Any())
            {
                return characters.Sum(character => (float)Formula.Calc(actor, Power) / character.Stats.MaxHp.CurrentValue);
            }
            else
            {
                return -1;
            }
        }
    }
}
