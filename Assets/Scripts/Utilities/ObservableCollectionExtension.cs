#nullable enable
using ObservableCollections;
using R3;
using System;
using System.Linq;

namespace Assets.Scripts.Utilities
{
    public static class ObservableCollectionExtension
    {
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
    }
}
