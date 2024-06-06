using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Algorithms
{
    public class AStarNode
    {
        private readonly Vector2Int _position;

        private AStarNode _rootNode;

        public int MoveTotalCost;

        public int ECost;

        public int Score => ECost + MoveTotalCost;

        public AStarNode(Vector2Int position, Vector2Int goal)
        {
            _position = position;
            SetEstimateCost(position, goal);
        }

        public void SetEstimateCost(Vector2Int position, Vector2Int goal)
        {
            var dx = Mathf.Abs(position.x - goal.x);
            var dy = Mathf.Abs(position.y - goal.y);
            ECost = Mathf.Max(dx, dy);
        }

        public void Open(AStarNode rootNode)
        {
            _rootNode = rootNode;
            if (_rootNode == null)
                MoveTotalCost = 0;
            else
                MoveTotalCost = _rootNode.MoveTotalCost + 1;
        }

        public List<Vector2Int> ToList()
        {
            var list = new List<Vector2Int>();
            list.Insert(0, _position);

            var node = _rootNode;
            while (node != null)
            {
                list.Insert(0, node._position);
                node = node._rootNode;
            }

            return list;
        }
    }
}