#nullable enable
using System;
using UnityEngine;
namespace Domain.Model
{
    [Serializable]
    public class Option<T>
    {
        public bool HasValue;
        [SerializeReference] private T? value;

        public T? Value
        {
            get => HasValue ? value : default;
            set
            {
                HasValue = value != null;
                this.value = value;
            }
        }

        public Option()
        {
            Value = default;
        }

        public Option(T? value)
        {
            Value = value;
        }

        public T Expect(string message)
        {
            if (!HasValue)
            {
                throw new NullReferenceException(message);
            }
            return value!;
        }

        public T Unwrap<TResult>()
        {
            return Expect("Value is null");
        }

        public Option<TResult> Select<TResult>(Func<T, TResult> func)
        {
            return new Option<TResult>(HasValue ? func(value!) : default);
        }

        public TResult SelectOrDefault<TResult>(Func<T, TResult> func, TResult defaultValue)
        {
            return HasValue ? func(value!) : defaultValue;
        }

        public TResult Match<TResult>(Func<T, TResult> func, Func<TResult> onNull)
        {
            if (HasValue)
            {
                return func(value!);
            }
            else
            {
                return onNull();
            }
        }
    }
}