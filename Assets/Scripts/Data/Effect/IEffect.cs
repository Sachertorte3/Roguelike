using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Data.Effect
{
    public interface IEffect : IHasInfo
    {
        public Impact Impact { get; }
        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, ISpawnPositionGenerator map);
        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target);
    }
    public interface ISpawnPositionGenerator
    {
        public HashSet<Vector2Int> GetAllPassablePositions();
    }
}

