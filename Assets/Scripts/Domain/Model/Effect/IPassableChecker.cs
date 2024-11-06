using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IPassableChecker
    {
        public IMapPosition At(Vector2Int position);
        public IEnumerable<IMapPosition> GetAllBlankPositionsOn(params EntityLayer[] layers);
        public IEnumerable<IMapPosition> GetAllBlankAndStandablePositionsOn(params EntityLayer[] layers);
        public IEnumerable<IMapPosition> GetAllWalkablePositions(IAffiliation affiliation);
    }
}