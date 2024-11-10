using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Entity;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface IActorOfEffect : IHasAffiliation, IEntity
    {
        public bool IsShiny { get; }
        public bool IsFlying { get; }
        public bool CanThroughWalls { get; }
        public string GetName(IPlayer player, bool ignoreVisibility = false);
        public IEnumerable<Vector2Int> VisibleArea { get; }
        public bool CanMove(Vector2Int position, Direction8 direction, IPassableChecker map);
        public bool CanMove(Direction8 direction, bool isFlying, bool canThroughWalls, IPassableChecker map);
        public bool CanMove(Direction8 direction, IPassableChecker map);
        public bool CanMoveIgnoreEntity(Direction8 direction, IPassableChecker map);
        public Aggression Aggression { get; }
        public int CurrentMaxHp { get; }
        public int CurrentHp { get; }
        public int GainHp(int value);
        public float GetElementAttackMultiplier(Element element);
    }
}