#nullable enable
using R3;
using System;

namespace Utilities.Messages
{
    public static class ObservableExtension
    {
        public static IDisposable RelayTo<TSender, TMessage>(this Observable<(TSender Item, TMessage Message)> source, MessageSubject<TSender, TMessage> target)
        {
            return source.Subscribe(item => target.OnNext(item));
        }
    }
}