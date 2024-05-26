using UnityEngine;

namespace Data
{
    public interface IActorOfEffect
    {
        public Vector2Int CurrentPosition { get; }
        public IAffiliation Affiliation { get; }
    }
    public interface IAffiliation
    {
        CharacterGroup Group { get; }
        bool IsAlly(IAffiliation other);
        bool IsEnemy(IAffiliation other);
    }
    public enum CharacterGroup
    {
        Player,
        Enemy,
        Neutral
    }
}

