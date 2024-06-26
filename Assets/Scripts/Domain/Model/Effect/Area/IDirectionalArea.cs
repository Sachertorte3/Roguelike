using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Domain.Model.Area
{
    public interface IDirectionalArea : IArea
    {
        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction);
    }
}