using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IEffect : IHasInfo
    {
        public Impact Impact { get; }
        public Color Color { get; }
        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map);
        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target);
    }

    public interface IPassableChecker
    {
        public HashSet<Vector2Int> GetAllPassablePositions();
        public bool IsPassable(Vector2Int position);
        public bool IsMapPassable(Vector2Int position);
    }
}