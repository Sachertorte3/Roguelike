using Domain.Model.Message;
using Domain.Service;
using ObservableCollections;
using R3;
using UnityEngine;

namespace Domain.Model.Character
{
    public interface IVisionRange
    {
        public IObservableCollection<Vector2Int> VisibleArea { get; }
        public Observable<OnVisibleAreaChangedMessage> OnVisibleAreaChanged { get; }
        public void Refrash(Vector2Int position, IMap world);
    }
}