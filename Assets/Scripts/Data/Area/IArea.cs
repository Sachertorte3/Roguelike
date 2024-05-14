using Scripts.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Data.Area
{
    public interface IArea: IHasInfo
    {
        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction);
    }
}