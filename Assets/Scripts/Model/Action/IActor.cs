using Cysharp.Threading.Tasks;
using Scripts.Model.Characters.Effect;
using Scripts.Utilities;
using UnityEngine;

namespace Scripts.Model.Action
{
    public interface IActor
    {
        public Vector2Int CurrentPosition { get; }
        public Direction8 CurrentDirection { get; }
        public bool CanMove(Direction8 direction);
        public UniTask Move(Direction8 direction);
        public UniTask UseSkill(Skill skill);
    }
}