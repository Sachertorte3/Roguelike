using Domain.Model.Character;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface IActorOfEffect : IHasAffiliation, IEntity
    {
        public bool IsShiny { get; }
        public string GetName(IHasAffiliation player);
        public bool CanMove(Vector2Int position, Direction8 direction, IPassableChecker map);
        public Aggression Aggression { get; }
        public int GainHp(int value);
        public float GetElementAttackMultiplier(Element element);
    }
}