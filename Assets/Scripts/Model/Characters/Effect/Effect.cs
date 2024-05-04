using Cysharp.Threading.Tasks;
using Scripts.Data.Area;
using Scripts.Model.Action;
using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Model.Characters.Effect
{
    public record Skill(int Power, IArea Area)
    {
        public UniTask Use(IActor actor)
        {
            IEnumerable<Vector2Int> area = Area.Get(actor.CurrentPosition, actor.CurrentDirection);
            GameManager.World.GetCharactersInArea(area.ToHashSet()).ForEach(character => character.Stats.Hp.Lose(Formula.Calc(actor, Power)));
            return UniTask.CompletedTask;
        }
    }
}
