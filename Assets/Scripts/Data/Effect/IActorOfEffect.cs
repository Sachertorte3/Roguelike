using UnityEngine;

namespace Data.Effect
{
    public interface IActorOfEffect
    {
        public Vector2Int CurrentPosition { get; }
        public IAffiliation Affiliation { get; }
    }
}