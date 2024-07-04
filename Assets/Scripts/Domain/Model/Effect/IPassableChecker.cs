using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IPassableChecker
    {
        public HashSet<Vector2Int> GetAllPassablePositions();
        public bool IsPassable(Vector2Int position);
        public bool IsMapPassable(Vector2Int position);
    }
}