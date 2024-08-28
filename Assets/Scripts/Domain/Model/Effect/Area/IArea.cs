using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Area
{
    public interface IArea : IHasInfo, IHasUpgrades
    {
        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction);
    }
}