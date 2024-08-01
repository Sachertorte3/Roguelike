#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Domain.Model
{
    [Serializable]
    public class NullableSerializable<T> where T : class
    {
        public bool HasValue;
        [SerializeField] private T? value;

        public T? Value
        {
            get => HasValue ? value : null;
            set
            {
                HasValue = value != null;
                this.value = value;
            }
        }

        public NullableSerializable()
        {
            Value = null;
        }

        public NullableSerializable(T? value)
        {
            Value = value;
        }

        public NullableSerializable<TResult> Select<TResult>(Func<T, TResult> func) where TResult : class
        {
            return new NullableSerializable<TResult>(HasValue ? func(value!) : null);
        }
    }
}