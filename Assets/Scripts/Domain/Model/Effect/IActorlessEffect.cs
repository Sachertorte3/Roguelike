using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Entity;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IActorlessEffect : IEffect
    {
        public UniTask Apply(ITargetOfEffect target, Vector2Int position, IMap map);
        public UniTask Apply(IEntity target, Vector2Int position, IMap map);
        public UniTask Apply(IEnumerable<Vector2Int> positions, IMap map);
    }
}