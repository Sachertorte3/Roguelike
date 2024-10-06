using System.Collections.Generic;
using Domain.Model.Map;
using Domain.Model.Message;
using R3;
using UnityEngine;

namespace Domain.Model.Character
{
    public interface IVisionRange
    {
        public bool IsClairvoyant { get; }
        public IReadOnlyCollection<Vector2Int> VisibleArea { get; }
        public Observable<OnVisibleAreaChangedMessage> OnVisibleAreaChanged { get; }
        public void Refresh(IMap map);
    }
}