#nullable enable
using Cysharp.Threading.Tasks;
using Scripts.Model.Action;
using Scripts.Model.Characters.Effect;
using Scripts.Utilities;

namespace Scripts.Model.Items
{
    public record Item(Skill Skill)
    {
        public async UniTask Use(IActor actor, Direction8 direction)
        {
            await Skill.Use(actor, direction);
        }
        public float Evaluate(IActor actor, Direction8 direction)
        {
            return Skill.Evaluate(actor, direction);
        }
    }
}
