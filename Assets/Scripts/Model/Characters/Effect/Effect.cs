using Cysharp.Threading.Tasks;
using Scripts.Data.Area;
using Scripts.Model.Action;
using Scripts.Utilities;
using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Model.Characters.Effect
{
    public record Skill(int Power, IArea Area)
    {
        public UniTask Use(IActor actor, Direction8 direction)
        {
            IEnumerable<Vector2Int> area = Area.Get(actor.CurrentPosition, direction);
            Globals.World.GetCharactersInArea(area.ToHashSet()).ForEach(character => character.Stats.Hp.Lose(Formula.Calc(actor, Power)));
            return UniTask.CompletedTask;
        }
        public float Evaluate(IActor actor, Direction8 direction)
        {
            IEnumerable<Vector2Int> area = Area.Get(actor.CurrentPosition, direction);
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
