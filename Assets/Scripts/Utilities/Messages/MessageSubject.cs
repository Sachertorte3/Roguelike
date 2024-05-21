#nullable enable
using R3;
using System;

namespace Utilities.Messages
{
    public class MessageSubject<TSender, TMessage>
    {
        private readonly Subject<(TSender Sender, TMessage Message)> _subject = new();
        public Observable<(TSender Sender, TMessage Message)> AsObservable() => _subject;
        public Observable<(T Sender, TMessage Message)> AsObservable<T>(Func<TSender, T> func) => _subject.Select(x => (func(x.Sender), x.Message));
        public void OnNext(TSender Sender, TMessage Message) => _subject.OnNext((Sender, Message));
        public void OnNext((TSender Sender, TMessage Message) tuple) => _subject.OnNext(tuple);
    }
}