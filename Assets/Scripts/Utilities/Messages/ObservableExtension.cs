#nullable enable
using System;
using R3;

namespace Utilities.Messages
{
    public static class ObservableExtension
    {
        public static IDisposable RelayTo<TSender, TMessage>(this MessageSubject<TSender, TMessage> source,
            Observer<(TSender, TMessage)> target)
        {
            return source.AsObservable().Subscribe(item => target.OnNext(item));
        }

        public static IDisposable RelayTo<TSender, TMessage>(this Observable<(TSender, TMessage)> source,
            MessageSubject<TSender, TMessage> target)
        {
            return source.Subscribe(item => target.OnNext(item));
        }
    }
}