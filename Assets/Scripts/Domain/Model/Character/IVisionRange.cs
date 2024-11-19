using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Domain.Model.Character
{
    public interface IVisionRange
    {
        public bool IsClairvoyant { get; }
        public IReadOnlyCollection<Vector2Int> VisibleArea { get; }
        public Observable<Unit> OnVisibleAreaChanged { get; }
        public void Refresh();
        public bool IsVisible(Vector2Int position);
    }
}