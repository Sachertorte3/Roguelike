#nullable enable
using System;
using UnityEngine;

namespace Utilities
{
    [Serializable]
    public class Option<T> where T : class
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

        public static Option<T> None => new();

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

        public T Unwrap()
        {
            return Expect("Value is null");
        }

        public Option<TResult> Select<TResult>(Func<T?, TResult> func) where TResult : class
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

    [Serializable]
    public class StructOption<T> where T : struct
    {
        public bool HasValue;
        [SerializeField] private T value;

        public T? Value
        {
            get => HasValue ? value : default;
            set
            {
                HasValue = value.HasValue;
                if (value.HasValue)
                {
                    this.value = value.Value;
                }
            }
        }

        public StructOption()
        {
            Value = default;
        }

        public static StructOption<T> None => new();

        public StructOption(T? value)
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

        public T Unwrap()
        {
            return Expect("Value is null");
        }

        public StructOption<TResult> Select<TResult>(Func<T?, TResult> func) where TResult : struct
        {
            return new StructOption<TResult>(HasValue ? func(value!) : null);
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