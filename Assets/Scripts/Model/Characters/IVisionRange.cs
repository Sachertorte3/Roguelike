using ObservableCollections;
using R3;
using System.Collections.Generic;
using UnityEngine;

namespace Model.Characters
{
    public interface IVisionRange
    {
        public IObservableCollection<Vector2Int> VisibleArea { get; }
        public Observable<OnVisibleAreaChangedMessage> OnVisibleAreaChanged { get; }
        public void Refrash(Vector2Int position);
    }
}