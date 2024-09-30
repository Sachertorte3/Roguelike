#nullable enable
using System;
using System.Threading.Tasks;
using UnityEngine;

public static class Option
{
    public static Option<T> None<T>() => new None<T>();
    public static Option<T> Some<T>(T value) => new Some<T>(value);
    
    public static Option<T> ToOption<T>(this T? value) where T : struct
    {
        if (value == null)
        {
            return Option<T>.None;
        }
        return new Some<T>(value.Value);
    }

    public static Option<T> ToOption<T>(this T? value) where T : class
    {
        if (value == null)
        {
            return Option<T>.None;
        }
        return new Some<T>(value);
    }
}

[Serializable]
public class Option<T> : IEquatable<Option<T>>
{
    public static Option<T> None => new None<T>();
    public bool IsNone => !hasValue;
    public bool IsSome => !IsNone;
    public bool HasValue => hasValue;

    [SerializeField] bool hasValue;
    [SerializeReference] T value;

    public T? Value => UnwrapOrNull();

    public Option()
    {
        hasValue = false;
        value = default(T);
    }
    public Option(T? value)
    {
        hasValue = value != null;
        if (hasValue)
            this.value = value!;
    }

    public bool Equals(Option<T> other)
    {
        if (ReferenceEquals(other, null))
            return false;
            
        if (hasValue != other.hasValue)
            return false;

        return !hasValue || Equals (value, other.value);
    }

    public override bool Equals(object obj)
    {
        if (obj is Option<T>)
            return Equals((Option<T>)obj);
        return false;
    }

    string FriendlyName(Type t)
    {
        return t.FullName;
    }

    public T Expect(string msg) => IsSome ? value : throw new Exception(msg);
    public T Unwrap() => IsSome ? value : throw new Exception($"Tried to unwrap a None<{FriendlyName(typeof(T))}>!");
    public T UnwrapOr(T def = default(T)) => IsSome ? value : def;
    public T UnwrapOr(Func<T> provider) => IsSome ? value : provider();
    public T? UnwrapOrNull() => IsSome ? value : default(T?);
    
    public Option<U> Map<U>(Func<T, U> converter) => IsSome ? new Option<U>(converter(value)) : new None<U>();
    public async Task<Option<U>> Map<U>(Func<T, Task<U>> converter) => IsSome ? new Option<U>(await converter(value)) : new None<U>();
    public U MapOr<U>(U def, Func<T, U> converter) => IsSome ? converter(value) : def;
    public U MapOr<U>(Func<U> provider, Func<T, U> converter) => IsSome ? converter(value) : provider();

    public Option<U> And<U>(Option<U> option) => IsNone ? Option<U>.None : option;
    public Option<U> AndThen<U>(Func<T, Option<U>> option) => IsNone ? Option<U>.None : option(value);
    public Task<Option<U>> AndThen<U>(Func<T, Task<Option<U>>> option) => IsNone ? Task.FromResult(Option<U>.None) : option(value);
    
    public Option<T> Or(Option<T> other) => IsSome ? this : other;
    public Option<T> OrElse(Func<Option<T>> option) => IsSome ? this : option();
    public Task<Option<T>> OrElse(Func<Task<Option<T>>> option) => IsSome ? Task.FromResult(this): option();

    public void Take()
    {
        value = default(T);
        hasValue = false;
    }

    public override int GetHashCode() => !hasValue ? 0 : (ReferenceEquals (value, null) ? -1 : value.GetHashCode());

    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);
    public static bool operator !=(Option<T> left, Option<T> right) => !left.Equals(right);
}

public class None<T> : Option<T> {}

public class Some<T> : Option<T>
{
    public Some(T value) : base (value) {}
}