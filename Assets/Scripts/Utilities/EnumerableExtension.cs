using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.Utilities
{
    public static class EnumerableExtension
    {
        public static IEnumerable<Vector2Int> RectRange(this RectInt rect)
        {
            for (int x = rect.x; x < rect.x + rect.width; x++)
            {
                for (int y = rect.y; y < rect.y + rect.height; y++)
                {
                    yield return new Vector2Int(x, y);
                }
            }
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
            if (!ie.Any())
            {
                throw new Exception("IEnumerable Argument is null or empty");
            }
            if (n > ie.Count())
            {
                throw new Exception("The number of elements to be retrieved is greater than the number of elements in IEnumerable");
            }
            List<T> result = new List<T>();

            T[] remaining = ie.ToArray(); // 残っている要素のリスト
            int remainingCount = remaining.Count();
            for (int i = 0; i < n; i++)
            {
                int index = randomRange(remainingCount); // ランダムに選択されたインデックス

                T element = remaining[index]; // ランダムに選択された要素
                result.Add(element); // ランダムに選択された要素のリストの末尾にランダムに選択された要素を追加する。

                remainingCount--; // 残っている要素のリストの末尾のインデックス
                T lastElement = remaining[remainingCount]; // 残っている要素のリストから末尾を削除する。
                if (index < remainingCount)
                { // ランダムに選択された要素が末尾以外なら…
                    remaining[index] = lastElement; // それを末尾の要素で置換する。
                }
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
    }
}
