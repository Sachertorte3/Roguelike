using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities.Algorithms
{
    public class AStar
    {
        private Dictionary<Vector2Int, bool> _passableMap;
        private Dictionary<Vector2Int, AStarNode> _map;
        private HashSet<Vector2Int> _openHash;
        private HashSet<Vector2Int> _closeHash;

        public AStar(Dictionary<Vector2Int, bool> passableMap)
        {
            SetMap(passableMap);
        }

        private void SetMap(Dictionary<Vector2Int, bool> passableMap)
        {
            _passableMap = passableMap;
            var length = passableMap.Count();
            _openHash = new HashSet<Vector2Int>();
            _closeHash = new HashSet<Vector2Int>();
            _map = new Dictionary<Vector2Int, AStarNode>(length);
        }

        public List<Vector2Int> Calc(Vector2Int start, Vector2Int goal)
        {
            _map.Add(start, new AStarNode(start, goal));

            _openHash.Clear();
            _closeHash.Clear();

            var current = start;
            _openHash.Add(current);
            _map[current].Open(null);
            var count = 1000;
            while (count-- > 0)
            {
                if (_openHash.Count <= 0)
                    return _map.Values.OrderBy(p => p.ECost).First().ToList();

                current = _openHash.OrderBy(p => _map[p].Score)
                    .First();

                if (_map[current].ECost <= 1)
                    break;

                _openHash.Remove(current);
                _closeHash.Add(current);

                OpenAround(current, goal);
            }

            return _map[current].ToList();
        }

        private void OpenAround(Vector2Int current, Vector2Int goal)
        {
            var openPos = new List<Vector2Int>
            {
                new Vector2Int(current.x + 1, current.y),
                new Vector2Int(current.x - 1, current.y),
                new Vector2Int(current.x, current.y + 1),
                new Vector2Int(current.x, current.y - 1),
                new Vector2Int(current.x + 1, current.y + 1),
                new Vector2Int(current.x + 1, current.y - 1),
                new Vector2Int(current.x - 1, current.y + 1),
                new Vector2Int(current.x - 1, current.y - 1),
            };
            foreach (var pos in openPos)
            {
                if (!_map.ContainsKey(pos))
                    if (inMap(pos) && _passableMap[pos])
                        _map.Add(pos, new AStarNode(pos, goal));
                    else
                        continue;

                if (_openHash.Contains(pos))
                {
                    continue;
                }

                if (_closeHash.Contains(pos))
                {
                    continue;
                }

                _openHash.Add(pos);
                _map[pos].Open(_map[current]);
            }
        }

        private bool inMap(Vector2Int position)
        {
            return _passableMap.ContainsKey(position);
        }
    }
}