using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utilities
{
    public static class EnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var item in ie)
            {
                action(item);
            }
        }

        public static IEnumerable<Vector2Int> RectRange(this RectInt rect)
        {
            for (var x = rect.x; x < rect.x + rect.width; x++)
            for (var y = rect.y; y < rect.y + rect.height; y++)
                yield return new Vector2Int(x, y);
        }

        public static IEnumerable<Vector2Int> CircleRange(Vector2Int center, float radius)
        {
            for (var x = -Mathf.FloorToInt(radius); x <= Mathf.FloorToInt(radius); x++)
            for (var y = -Mathf.FloorToInt(radius); y <= Mathf.FloorToInt(radius); y++)
                if (x * x + y * y <= radius * radius)
                    yield return new Vector2Int(x, y) + center;
        }

        public static T GetAtRandom<T>(this IEnumerable<T> ie)
        {
            return GetAtRandom(ie, 1, max => Random.Range(0, max))[0];
        }

        public static List<T> GetAtRandom<T>(this IEnumerable<T> ie, int n)
        {
            return GetAtRandom(ie, n, max => Random.Range(0, max));
        }

        public static List<T> GetAtRandom<T>(this IEnumerable<T> ie, int n, Func<int, int> randomRange)
        {
            if (!ie.Any()) throw new Exception("IEnumerable Argument is null or empty");
            if (n > ie.Count())
                throw new Exception(
                    "The number of elements to be retrieved is greater than the number of elements in IEnumerable");
            List<T> result = new();

            var remaining = ie.ToArray(); // 残っている要素のリスト
            var remainingCount = remaining.Count();
            for (var i = 0; i < n; i++)
            {
                var index = randomRange(remainingCount); // ランダムに選択されたインデックス

                var element = remaining[index]; // ランダムに選択された要素
                result.Add(element); // ランダムに選択された要素のリストの末尾にランダムに選択された要素を追加する。

                remainingCount--; // 残っている要素のリストの末尾のインデックス
                var lastElement = remaining[remainingCount]; // 残っている要素のリストから末尾を削除する。
                if (index < remainingCount)
                    // ランダムに選択された要素が末尾以外なら…
                    remaining[index] = lastElement; // それを末尾の要素で置換する。
            }

            return result;
        }

        public static T MinBy<T, U>(this IEnumerable<T> xs, Func<T, U> key) where U : IComparable<U>
        {
            return xs.Aggregate((a, b) => key(a).CompareTo(key(b)) < 0 ? a : b);
        }

        public static T MaxBy<T, U>(this IEnumerable<T> xs, Func<T, U> key) where U : IComparable<U>
        {
            return xs.Aggregate((a, b) => key(a).CompareTo(key(b)) > 0 ? a : b);
        }

        public static void SynchronizeWith<T>(this ICollection<T> collectionA, IEnumerable<T> collectionB)
        {
            // コレクションAの要素のうち、コレクションBに存在しないものを削除する
            var itemsToRemove = collectionA.Except(collectionB).ToList();
            foreach (var item in itemsToRemove)
            {
                collectionA.Remove(item);
            }

            // コレクションBの要素のうち、コレクションAに存在しないものを追加する
            var itemsToAdd = collectionB.Except(collectionA).ToList();
            foreach (var item in itemsToAdd)
            {
                collectionA.Add(item);
            }
        }
        public static IEnumerable<T> CreateArrayWithNewInstances<T>(int count) where T : new()
        {
            return Enumerable.Range(0, count).Select(_ => new T());
        }
    }
}