using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Character;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface ICharacterSkill : ISerializable<CharacterSkillMemento>, ISkill, IHasInfo
    {
        public void UpdateTurn();
        public bool IsUsable();
        public Color Color { get; }
        public int RushDistance { get; }
        public IEnumerable<Vector2Int> GetArea(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IEffectMap map);
        public UniTask<bool> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map);
        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap world);
    }
}