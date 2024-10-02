#nullable enable
using System;

namespace Utilities
{
    [Serializable]
    public class Id<T>
    {
        public static Id<T> Empty => new(Guid.Empty);
        public Guid Value { get; }
        public Id(Guid value) => Value = value;
        public Id(string value) => Value = Guid.Parse(value);
        public static Id<T> Generate() => new(Guid.NewGuid());
        public override string ToString() => Value.ToString();
        public static bool operator ==(Id<T>? a, Id<T>? b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Value == b.Value;
        }
        public static bool operator !=(Id<T>? a, Id<T>? b) => !(a == b);
        public override bool Equals(object? obj) => obj is Id<T> id && Value == id.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }
}