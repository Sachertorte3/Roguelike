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
        public HashSet<Vector2Int> GetAllPassablePositions(IAffiliation affiliation);
        public bool IsBlank(Vector2Int position, params EntityLayer[] layers);
        public bool IsBlankAndStandable(Vector2Int position, params EntityLayer[] layers);
        public bool IsPassable(Vector2Int position, IAffiliation affiliation);
        public bool IsPassableOnMap(Vector2Int position);
    }
}