using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Data.Condition
{
    public interface IConditionData
    {
        public string Name { get; }
        public void Inflict(IHasCondition hasCondition);
        public UniTask Persist(IHasCondition hasCondition);
        public void Delete(IHasCondition hasCondition);
    }
    public interface IHasCondition
    {
        public UniTask LoseHp(int value);
    }
}
