#nullable enable
using R3;
using System;

namespace Utilities.Messages
{
    public class MessageSubject<TSender, TMessage> : Observable<(TSender Sender, TMessage Message)>
    {
        private readonly Subject<(TSender Sender, TMessage Message)> _subject = new();
        public Observable<(T Sender, TMessage Message)> SelectSender<T>(Func<TSender, T> func) => _subject.Select(x => (func(x.Sender), x.Message));
        public void OnNext(TSender Sender, TMessage Message) => _subject.OnNext((Sender, Message));
        public void OnNext((TSender Sender, TMessage Message) tuple) => _subject.OnNext(tuple);

        protected override IDisposable SubscribeCore(Observer<(TSender Sender, TMessage Message)> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}