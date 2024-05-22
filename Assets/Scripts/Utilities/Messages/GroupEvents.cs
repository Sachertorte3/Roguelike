#nullable enable
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Messages
{
    public class GroupEvents<TSender>
    {
        private Dictionary<Type, object> _events = new();
        private Dictionary<object, CompositeDisposable> _disposable = new();
        public void Add<TMessage>(TSender sender, Observable<TMessage> observable)
        {
            if (!_disposable.ContainsKey(sender))
            {
                _disposable[sender] = new CompositeDisposable();
            }
            _disposable[sender].Add(observable.Select(message => (sender, message)).RelayTo(GetSubject<TMessage>()));
        }
        public void Add<TMessage>(object obj, Observable<(TSender, TMessage)> observable)
        {
            if (!_disposable.ContainsKey(obj))
            {
                _disposable[obj] = new CompositeDisposable();
            }
            _disposable[obj].Add(observable.RelayTo(GetSubject<TMessage>()));
        }
        public void Remove(object sender)
        {
            _disposable[sender].Dispose();
            _disposable.Remove(sender);
        }
        public MessageSubject<TSender, TMessage> GetSubject<TMessage>()
        {
            if (!_events.ContainsKey(typeof(TMessage)))
            {
                _events[typeof(TMessage)] = new MessageSubject<TSender, TMessage>();
            }
            return (MessageSubject<TSender, TMessage>)_events[typeof(TMessage)];
        }
        public Observable<(TSender, TMessage)> GetObservable<TMessage>()
        {
            if (!_events.ContainsKey(typeof(TMessage)))
            {
                _events[typeof(TMessage)] = new MessageSubject<TSender, TMessage>();
            }
            return ((MessageSubject<TSender, TMessage>)_events[typeof(TMessage)]).AsObservable();
        }
    }
}