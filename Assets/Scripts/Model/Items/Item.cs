#nullable enable
using Cysharp.Threading.Tasks;
using Scripts.Data;
using Scripts.Model.Action;
using Scripts.Model.Characters.Effect;
using Scripts.Utilities;
using UnityEngine;

namespace Scripts.Model.Items
{
    public record Item(ItemData Data)
    {
        public Sprite Icon => Data.Sprite;
        public async UniTask Use(IActor actor, Direction8 direction)
        {
            await new Skill(Data.Skill).Use(actor, direction);
        }
        public float Evaluate(IActor actor, Direction8 direction)
        {
            return new Skill(Data.Skill).Evaluate(actor, direction);
        }
    }
}
