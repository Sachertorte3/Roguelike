using System.Collections.Generic;
using Domain.Model.Message;
using Domain.Service;
using R3;
using UnityEngine;

namespace Domain.Model.Character
{
    public interface IVisionRange
    {
        public IReadOnlyCollection<Vector2Int> VisibleArea { get; }
        public Observable<OnVisibleAreaChangedMessage> OnVisibleAreaChanged { get; }
        public void Refresh(Vector2Int position, IMap world);
    }
}