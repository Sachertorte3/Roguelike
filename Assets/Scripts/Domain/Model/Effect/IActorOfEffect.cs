using System.Collections.Generic;
using Domain.Model.Character;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface IActorOfEffect : IHasAffiliation, IEntity
    {
        public bool IsShiny { get; }
        public bool IsFlying { get; }
        public string GetName(IHasAffiliation player, bool ignoreVisibility = false);
        public IEnumerable<Vector2Int> VisibleArea { get; }
        public bool CanMove(Vector2Int position, Direction8 direction, IPassableChecker map);
        public bool CanMove(Direction8 direction, bool isFlying, IPassableChecker map);
        public bool CanMove(Direction8 direction, IPassableChecker map);
        public Aggression Aggression { get; }
        public int GainHp(int value);
        public float GetElementAttackMultiplier(Element element);
    }
}