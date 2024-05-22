#nullable enable
using ObservableCollections;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
    public static class ObservableExtension
    {
        public static IDisposable RelayTo<T>(this Observable<T> source, Observer<T> target)
        {
            return source.Subscribe(item => target.OnNext(item));
        }
        public static IDisposable SubscribeToAll<T>(this IObservableCollection<T> list, Action<T> addAction, Action<T> removeAction = null)
        {
            foreach (var item in list)
            {
                addAction(item);
            }

            return new CompositeDisposable() {
                list.ObserveAdd()
                    .Select(i => i.Value)
                    .Subscribe(addAction),
                list.ObserveRemove()
                    .Select(i => i.Value)
                    .Subscribe(i => removeAction?.Invoke(i)),
                list.ObserveReplace()
                    .Subscribe(value =>
                    {
                        addAction(value.NewValue);
                        removeAction?.Invoke(value.OldValue);
                    })
            };
        }
        public static IDisposable SubscribeToAll<T>(this ReadOnlyReactiveProperty<T> property, Action<T> addAction, Action<T> removeAction = null)
        {
            addAction(property.CurrentValue);

            return property.Pairwise()
                .Subscribe(value =>
                {
                    addAction(value.Current);
                    removeAction?.Invoke(value.Previous);
                });
        }
        public static void SynchronizeWith<T>(this ObservableHashSet<T> collectionA, IEnumerable<T> collectionB) where T : notnull
        {
            // コレクションAの要素のうち、コレクションBに存在しないものを削除する
            var itemsToRemove = collectionA.Except(collectionB).ToList();
            foreach (var item in itemsToRemove)
            {
                collectionA.Remove(item);
            }

            // コレクションBの要素のうち、コレクションAに存在しないものを追加する
            var itemsToAdd = collectionB.Except(collectionA).ToList();
            foreach (var item in itemsToAdd)
            {
                collectionA.Add(item);
            }
        }
        public static IDisposable LiveSynchronizeWith<T>(this ObservableHashSet<T> collectionA, IObservableCollection<T> collectionB) where T : notnull
        {
            collectionA.SynchronizeWith(collectionB);

            return collectionB.SubscribeToAll(add => collectionA.Add(add), remove => collectionA.Remove(remove));
        }
        public static IDisposable LiveSynchronizeWith<T>(this ICollection<T> collectionA, IObservableCollection<T> collectionB) where T : notnull
        {
            collectionA.SynchronizeWith(collectionB);

            return collectionB.SubscribeToAll(add => collectionA.Add(add), remove => collectionA.Remove(remove));
        }
    }
}
