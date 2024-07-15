using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IEffect : IHasInfo
    {
        public Impact Impact { get; }
        public Color Color { get; }
        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map) => UniTask.CompletedTask;
        public UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map) => UniTask.CompletedTask;
        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target);
    }
}