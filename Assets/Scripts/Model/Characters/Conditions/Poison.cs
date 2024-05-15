using Cysharp.Threading.Tasks;
using Data.Condition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utilities;

namespace Model.Characters.Conditions
{
    internal class Poison : IConditionData
    {
        public string Name => "毒";
        public ParticleType ParticleType => ParticleType.PoisoningBubble;

        public void Inflict(IHasCondition hasCondition)
        {
            
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            hasCondition.LoseHp(1);
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {

        }
    }
}
