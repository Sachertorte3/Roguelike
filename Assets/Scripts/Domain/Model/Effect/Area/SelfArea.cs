using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Area
{
    public class SelfArea : INotDirectionalArea
    {
        public IEnumerable<Vector2Int> Get(Vector2Int position)
        {
            return new List<Vector2Int> { position };
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction)
        {
            return Get(position);
        }

        public string Info()
        {
            return "その場";
        }
    }
}