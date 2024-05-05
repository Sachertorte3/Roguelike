using R3;
using Scripts.Utilities;
using System;
using UnityEngine;

namespace Scripts.View
{
    public class CharacterView : MonoBehaviour
    {
        public int MoveMilliseconds = 1000;
        public int DashMilliseconds = 1000;
        private const int frame = 16;
        private Func<bool> _isDash;
        public void Construct(InputReceiver receiver)
        {
            _isDash = () => receiver.IsDash;
        }
        public void Move(Vector2Int destination, Direction8 direction)
        {
            Vector3Int position = (Vector3Int)destination - (Vector3Int)direction.Vector();
            Observable.Interval(TimeSpan.FromSeconds((_isDash() ? DashMilliseconds : MoveMilliseconds) / 1000f * 0.75f / frame))
            .Take(frame)
            .Index()
            .Subscribe(l =>
            {
                transform.position = Vector3.Lerp(position, (Vector3Int)destination, (l + 1) / (float)frame);
            }).AddTo(this);
        }
    }
}