using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface ISkill : ISerializable<SkillMemento>, IHasInfo
    {
        public Color Color { get; }

        public IEnumerable<Vector2Int> GetArea(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IEffectMap map);

        public UniTask Use(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map);
        public float Evaluate(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap world);
    }
}