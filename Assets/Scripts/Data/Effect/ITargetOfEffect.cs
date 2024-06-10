using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data.Condition;
using UnityEngine;
using Utilities;

namespace Data.Effect
{
    public interface ITargetOfEffect
    {
        public Vector2Int CurrentPosition { get; }
        public int CurrentMaxHp { get; }
        public int CurrentHp { get; }
        public UniTask GainHp(int value);
        public UniTask LoseHp(int value);
        public UniTask BlowAway(Direction8 direction, int distance, IPassableChecker map);
        public void Teleport(Vector2Int position);
        public void AddCondition(IConditionData condition, RemovalConditionData removalCondition);
    }
}

