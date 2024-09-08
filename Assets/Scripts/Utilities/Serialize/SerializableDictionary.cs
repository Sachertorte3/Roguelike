#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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
        public SerializableDictionary() { }
        public SerializableDictionary(Dictionary<TKey, TValue> dictionary)
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
    public static class SerializableDictionaryExtension
    {
        public static SerializableDictionary<TKey, TValue> ToSerializable<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return new SerializableDictionary<TKey, TValue>(dictionary);
        }
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this SerializableDictionary<TKey, TValue> serializableDictionary)
        {
            return new Dictionary<TKey, TValue>(serializableDictionary);
        }

        public static SerializableDictionary<TKey, TSource> ToSerializableDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.ToDictionary(keySelector).ToSerializable();
        }
        public static SerializableDictionary<TKey, TSource> ToSerializableDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return source.ToDictionary(keySelector, comparer).ToSerializable();
        }
        public static SerializableDictionary<TKey, TElement> ToSerializableDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return source.ToDictionary(keySelector, elementSelector).ToSerializable();
        }
        public static SerializableDictionary<TKey, TElement> ToSerializableDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            return source.ToDictionary(keySelector, elementSelector, comparer).ToSerializable();
        }
    }
}