using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Area
{
    public interface INotDirectionalArea : IArea
    {
        public IEnumerable<Vector2Int> Get(Vector2Int position);
    }
}