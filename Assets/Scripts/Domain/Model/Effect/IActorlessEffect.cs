using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IActorlessEffect : IEffect
    {
        public UniTask Apply(ITargetOfEffect target, IMap map) => UniTask.CompletedTask;
        public UniTask Apply(IEntity target, IMap map) => UniTask.CompletedTask;
        public UniTask Apply(IEnumerable<Vector2Int> positions, IMap map) => UniTask.CompletedTask;
    }
}