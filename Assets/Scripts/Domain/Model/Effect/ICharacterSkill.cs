using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Map;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface ICharacterSkillWithRule
    {
        public ICharacterSkill Skill { get; }
        public int Priority { get; }
    }
    public interface ICharacterSkill : ISkill, IHasInfo
    {
        public void CoolDown();
        public Color Color { get; }
        public int ChargeTurn { get; }
        public int RushDistance { get; }
        public int BackStepDistance { get; }
        public IEnumerable<Vector2Int> GetArea(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map, bool onlyVisible = false);
        public UniTask<ISkillResult> Use(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map);
        public float Evaluate(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map);
    }
}