using Scripts.Utilities;
using System;
using UniRx;
using UnityEngine;

namespace Scripts.View
{
    public class CharacterView : MonoBehaviour
    {
        public IObserver<(Vector2Int destination, Direction8 direction)> OnMove => _onMove;
        private Subject<(Vector2Int destination, Direction8 direction)> _onMove = new Subject<(Vector2Int destination, Direction8 direction)>();
        public int MoveMilliseconds = 1000;
        private const int frame = 16;
        private void Start()
        {
            _onMove.Subscribe(onMove =>
            {
                Vector3Int position = (Vector3Int)onMove.destination - (Vector3Int)onMove.direction.Vector();
                Vector3Int destination = (Vector3Int)onMove.destination;
                Observable.Interval(TimeSpan.FromSeconds(MoveMilliseconds / 1000f * 0.75f / frame))
                .Take(frame)
                .Subscribe(l =>
                {
                    transform.position = Vector3.Lerp(position, destination, (l+1) / (float)frame);
                }).AddTo(this);
            }).AddTo(this);
        }
    }
}