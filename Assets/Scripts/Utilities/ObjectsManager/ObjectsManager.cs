using ObservableCollections;
using R3;
using R3.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities.ObjectsManager
{
    public static class ObjectsManager
    {
        private static readonly Dictionary<Type, ObservableHashSet<object>> _caches = new();
        public static HashSet<object> _objects = new();

        public static void RegisterType<T>()
        {
            if (!_caches.ContainsKey(typeof(T)))
            {
                _caches.Add(typeof(T), new ObservableHashSet<object>());
                foreach (var obj in _objects)
                    if (typeof(T).IsInstanceOfType(obj))
                        _caches[typeof(T)].Add(obj);
            }
        }

        public static void RegisterInstance(object instance)
        {
            foreach (var pair in _caches)
                if (pair.Key.IsInstanceOfType(instance))
                    _caches[pair.Key].Add(instance);
            _objects.Add(instance);
        }

        public static void UnregisterInstance(object instance)
        {
            foreach (var pair in _caches) pair.Value.Remove(instance);
            _objects.Remove(instance);
        }

        public static T Register<T>(this T obj) where T : IDestroyObservable
        {
            RegisterInstance(obj);
            obj.OnDestroy += () => UnregisterInstance(obj);
            return obj;
        }

        public static T RegisterComponent<T>(this T obj) where T : Component
        {
            RegisterInstance(obj);
            obj.OnDestroyAsObservable().Subscribe(_ => UnregisterInstance(obj));
            return obj;
        }

        public static IEnumerable<T> GetObjectsByType<T>()
        {
            if (!_caches.ContainsKey(typeof(T))) RegisterType<T>();
            return _caches[typeof(T)].Cast<T>();
        }

        public static Observable<T> ObserveAdd<T>()
        {
            if (!_caches.ContainsKey(typeof(T))) RegisterType<T>();
            return _caches[typeof(T)].ObserveAdd().Select(obj => (T)obj.Value);
        }
    }
}