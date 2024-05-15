using Cysharp.Threading.Tasks;
using Data.Condition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Model.Characters.Conditions
{
    internal class Poison : IConditionData
    {
        public string Name => "毒";

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
