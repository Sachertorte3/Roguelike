using System.Collections.Generic;
using Domain.Model.Character;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IPassableChecker
    {
        public ICharacter Player { get; }
        public HashSet<Vector2Int> GetAllBlankPositionsOn(params EntityLayer[] layers);
        public HashSet<Vector2Int> GetAllBlankAndStandablePositionsOn(params EntityLayer[] layers);
        public HashSet<Vector2Int> GetAllWalkablePositions(IAffiliation affiliation);
        public bool CanPlace(Vector2Int position, bool isFlying, bool canIgnoreWall, bool ignoreEntity);
        public bool IsInside(Vector2Int position);
        public bool IsBlankIgnoreWall(Vector2Int position, params EntityLayer[] layers);
        public bool IsBlank(Vector2Int position, params EntityLayer[] layers);
        public bool IsBlankAndStandable(Vector2Int position, params EntityLayer[] layers);
        public bool IsWalkable(Vector2Int position, IAffiliation affiliation);
        public bool IsWalkableOnMap(Vector2Int position);
        public bool IsPassableOnMap(Vector2Int position);
    }
}