using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Data.Area
{
    public interface IArea : IHasInfo
    {
        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction);
    }
}