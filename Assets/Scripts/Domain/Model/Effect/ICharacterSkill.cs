using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Model.Memento;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface ICharacterSkillWithRule
    {
        public ICharacterSkill Skill { get; }
        public int Priority { get; }
    }
    public interface ICharacterSkill : ISerializable<CharacterSkillMemento>, ISkill, IHasInfo
    {
        public void CoolDown();
        public bool IsUsable();
        public Color Color { get; }
        public int ChargeTurn { get; }
        public int RushDistance { get; }
        public int BackStepDistance { get; }
        public IEnumerable<Vector2Int> GetArea(IActor actor, Vector2Int position, Direction8 direction, IMap map, bool onlyVisible = false);
        public UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map);
        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap map);
    }
}