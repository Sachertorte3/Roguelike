using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Scripts.Model.Characters
{
    public interface IVisionRange
    {
        public Observable<HashSet<Vector2Int>> OnVisibleAreaChanged { get; }
        public void Refrash(Vector2Int position);
        public HashSet<Vector2Int> Get();
    }
}
