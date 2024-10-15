#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<SerializableKeyValuePair> data = new();

        [Serializable]
        public class SerializableKeyValuePair
        {
            [SerializeField] public TKey Key;
            [SerializeField] public TValue Value;

            public SerializableKeyValuePair(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        public SerializableDictionary()
        {
        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            foreach (var pair in dictionary)
            {
                this[pair.Key] = pair.Value;
            }
        }

        public void OnBeforeSerialize()
        {
            data.Clear();
            using var e = GetEnumerator();
            while (e.MoveNext())
            {
                data.Add(new SerializableKeyValuePair(e.Current.Key, e.Current.Value));
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            foreach (var pair in data)
            {
                this[pair.Key] = pair.Value;
            }
        }
    }
}