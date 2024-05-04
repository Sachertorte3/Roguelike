using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Data.Area
{
    public interface INotDirectionalArea : IArea
    {
        public IEnumerable<Vector2Int> Get(Vector2Int position);
    }
}