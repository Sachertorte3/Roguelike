using Cysharp.Threading.Tasks;
using Scripts.Model.Characters.Effect;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Model.Action
{
    internal record UseSkill(Skill Skill) : IAction
    {
        private float score;
        public bool Doable(IActor actor)
        {
            return true;
        }
        public async UniTask Do(IActor actor)
        {
            actor.UseSkill(Skill);
        }
        public float Evaluate(IActor actor)
        {
            score = 1;
            return score;
        }
    }
}
