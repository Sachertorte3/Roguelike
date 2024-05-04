using Cysharp.Threading.Tasks;
using Scripts.Data;
using Scripts.Model.Action;
using Sirenix.Utilities;

namespace Scripts.Model.Characters.Effect
{
    public record Skill(int power)
    {
        public LineArea AreaData = new LineArea(1);
        public UniTask Use(IActor actor)
        {
            GameManager.World.GetCharactersInArea(AreaData.Get(actor.CurrentPosition, actor.CurrentDirection)).ForEach(character => character.Stats.Hp.Lose(Formula.Calc(actor, power)));
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
