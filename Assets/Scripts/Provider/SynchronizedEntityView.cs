#nullable enable
using BidirectionalMap;
using Domain.Model.Setting;
using Domain.Service.Entities;
using R3;
using UnityEngine;
using Utilities.ObjectsManager;
using View;
using Object = UnityEngine.Object;

namespace Provider
{
    public abstract class SynchronizedEntityView<T, TView> where T : IEntity where TView : Component
    {
        private readonly BiMap<T, TView> _viewDict = new();
        protected abstract TView _viewPrefab { get; }
        protected abstract InputReceiver _inputReceiver { get; init; }

        protected abstract EntityView GetEntityView(TView view);
        public virtual void Add(T obj)
        {
            var view = Object.Instantiate(_viewPrefab);
            _viewDict.Add(obj, view);
            InitializeView(obj, view);
            ConstructEntity(obj, GetEntityView(view));
        }

        public virtual void Remove(T obj)
        {
            CleanupView(obj, Get(obj));
            Object.Destroy(Get(obj).gameObject);
            _viewDict.Remove(obj);
        }

        protected abstract void InitializeView(T item, TView view);
        protected abstract void CleanupView(T item, TView view);

        public T Get(TView view)
        {
            return _viewDict.Reverse[view];
        }

        public TView Get(T obj)
        {
            return _viewDict.Forward[obj];
        }

        public void ConstructEntity(IEntity entity, EntityView entityView)
        {
            entityView.Construct(_inputReceiver);
            entity.OnMove.Subscribe(move => entityView.Move(move.destination, move.direction)).AddTo(entityView);
            entity.OnTeleport.Subscribe(teleport => entityView.Teleport(teleport)).AddTo(entityView);
            Settings.ThrowMilliseconds.Subscribe(value => entityView.SetMoveMilliseconds(value)).AddTo(entityView);
            Settings.ThrowMilliseconds.Subscribe(value => entityView.SetDashMilliseconds(value)).AddTo(entityView);

            var spriteView = entityView.GetComponent<SpriteView>();
            spriteView.RegisterComponent();
            spriteView.transform.position = (Vector3Int)entity.CurrentPosition;
            entity.Visibility.Subscribe(visibility => spriteView.SetVisibility(visibility)).AddTo(spriteView);
        }
    }
}