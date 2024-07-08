using System;

namespace Utilities
{
    public static class UniqueIdGenerator
    {
        private static int _id = new Random().Next();
        public static Id<T> Generate<T>()
        {
            return new Id<T>(unchecked(_id++));
        }
    }
    public record Id<T>(int Value);
}