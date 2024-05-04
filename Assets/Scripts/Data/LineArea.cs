using Scripts.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Data
{
    public record LineArea(int Length): IDirectionalArea
    {
        public HashSet<Vector2Int> Get(Vector2Int position, Direction8 direction)
        {
            return Enumerable.Range(1, Length).Select(i => position + direction.Vector() * i).ToHashSet();
        }
    }
    public interface IDirectionalArea
    {
        public HashSet<Vector2Int> Get(Vector2Int position, Direction8 direction);
    }
    public interface INotDirectionalArea
    {
        public HashSet<Vector2Int> Get(Vector2Int position);
    }
}