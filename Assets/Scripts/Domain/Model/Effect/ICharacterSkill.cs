using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Map;
using Domain.Model.Memento;
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
        public int BackStepDistance { get; }
        public UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map);
        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap world);
    }
}