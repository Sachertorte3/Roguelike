using Cysharp.Threading.Tasks;
using Data.Condition;
using UnityEngine;

namespace Data.Effect
{
    public interface ITargetOfEffect
    {
        public int CurrentMaxHp { get; }
        public int CurrentHp { get; }
        public UniTask GainHp(int value);
        public UniTask LoseHp(int value);
        public void Teleport(Vector2Int position);
        public void AddCondition(IConditionData condition, RemovalConditionData removalCondition);

    }
}