#nullable enable
using BidirectionalMap;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Provider
{
    public abstract class SynchronizedView<T, TView> where TView : Component
    {
        private readonly BiMap<T, TView> _viewDict = new();
        protected abstract TView _viewPrefab { get; }

        public void Add(T obj)
        {
            var view = Object.Instantiate(_viewPrefab);
            _viewDict.Add(obj, view);
            InitializeView(obj, view);
        }

        public void Remove(T obj)
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
    }
}