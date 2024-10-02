using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities.Algorithms
{
    public class AStar
    {
        private HashSet<Vector2Int> _closeHash;
        private Dictionary<Vector2Int, AStarNode> _map;
        private HashSet<Vector2Int> _openHash;
        private HashSet<Vector2Int> _passables;

        public AStar(HashSet<Vector2Int> passables)
        {
            SetMap(passables);
        }

        private void SetMap(HashSet<Vector2Int> passables)
        {
            _passables = passables;
            var length = passables.Count;
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
            var count = 100;
            while (count-- > 0)
            {
                if (_openHash.Count <= 0)
                    return _map.Values.OrderBy(p => p.ECost).First().ToList();

                current = _openHash.OrderBy(p => _map[p].Score)
                    .First();

                if (_passables.Contains(goal))
                {
                    if (_map[current].ECost <= 0)
                        break;
                }
                else
                {
                    if (_map[current].ECost <= 1)
                        break;
                }

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
                new(current.x + 1, current.y),
                new(current.x - 1, current.y),
                new(current.x, current.y + 1),
                new(current.x, current.y - 1),
                new(current.x + 1, current.y + 1),
                new(current.x + 1, current.y - 1),
                new(current.x - 1, current.y + 1),
                new(current.x - 1, current.y - 1)
            };
            foreach (var pos in openPos)
            {
                if (!_map.ContainsKey(pos))
                    if (CanMove(current, pos))
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

        private bool CanMove(Vector2Int current, Vector2Int pos)
        {
            if (IsDiagonalMove(current, pos) && !CanMoveDiagonally(current, pos))
            {
                return false;
            }
            return _passables.Contains(pos);
        }

        private bool IsDiagonalMove(Vector2Int current, Vector2Int pos)
        {
            return Mathf.Abs(current.x - pos.x) == 1 && Mathf.Abs(current.y - pos.y) == 1;
        }

        private bool CanMoveDiagonally(Vector2Int current, Vector2Int pos)
        {
            var horizontal = new Vector2Int(pos.x, current.y);
            var vertical = new Vector2Int(current.x, pos.y);
            return _passables.Contains(horizontal) && _passables.Contains(vertical);
        }
    }
}