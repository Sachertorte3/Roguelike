using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Scripts.Model
{
    public sealed class Character
    {
        private IReadOnlyReactiveProperty<Vector2Int> _position;
        public void Move(Direction8 direction)
        {

        }
        public void Teleport(Vector2Int position)
        {

        }
    }
}