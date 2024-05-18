using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Model.Characters
{
    public interface IVisionRange
    {
        public Observable<OnVisibleAreaChangedMessage> OnVisibleAreaChanged { get; }
        public void Refrash(Vector2Int position);
        public HashSet<Vector2Int> Get();
    }
}