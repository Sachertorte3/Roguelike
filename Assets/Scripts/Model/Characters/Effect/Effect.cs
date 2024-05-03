using Cysharp.Threading.Tasks;
using Scripts.Model.Action;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Model.Characters.Effect
{
    internal record WorldEffect(HashSet<Vector2Int> Area, Skill Skill)
    {
        public async UniTask Spawn(IActor actor)
        {
            await GameManager.World.GetCharactersInArea(Area).Select(character => Skill.Use(actor, character));
        }
    }
    public record Skill(int power)
    {
        public UniTask Use(IActor actor, ITarget target)
        {
            target.Stats.Hp.Lose(Formula.Calc(actor, power));
            return UniTask.CompletedTask;
        }
    }
    internal static class Formula
    {
        public static int Calc(IActor actor, int power)
        {
            return power;
        }
    }
}
