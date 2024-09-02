using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Effect.Area
{
    public interface INotDirectionalArea : IArea
    {
        public IEnumerable<Vector2Int> Get(Vector2Int position, IMap map);
    }
}