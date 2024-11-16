#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utilities
{
    public static class RandUtils
    {
        // Box-Muller法を使用して正規分布からのサンプリングを行う
        private static float Normal(float mu, float sigma)
        {
            var u1 = Random.value; // 0から1までの適当な数字を1つ取る
            var u2 = Random.value; // もう一度、0から1までの適当な数字を1つ取る

            // Box-Muller法で乱数を取得する
            var randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                                Mathf.Sin(2.0f * Mathf.PI * u2);

            // 結果を計算する
            return mu + sigma * randStdNormal;
        }

        public static int Binomial(float trials, float probability)
        {
            // 二項分布の平均と標準偏差を計算
            var mean = trials * probability;
            var stdDev = Mathf.Sqrt(trials * probability * (1 - probability));

            // 正規分布からのサンプリング
            var normalValue = Normal(mean, stdDev);

            // 結果を整数に四捨五入
            return Mathf.RoundToInt(normalValue);
        }

        public static float LogNormal(float mu, float sigma)
        {
            return Mathf.Exp(Normal(mu, sigma));
        }

        public static float LogNormalFromMean(float mean, float sigma)
        {
            return Mathf.Exp(Normal(Mathf.Log(mean), sigma));
        }

        public static bool IsChance(float probability)
        {
            return Random.value < probability;
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

        public static int WeightedIndex(this IEnumerable<float> source)
        {
            return WeightedIndex(source, Random.value);
        }

        public static int WeightedIndex(this IEnumerable<float> source, float value)
        {
            var weights = source.ToArray();

            var total = weights.Sum(x => x);
            if (total <= 0f)
            {
                return -1;
            }

            var i = 0;
            var w = 0f;
            foreach (var weight in weights)
            {
                w += weight / total;
                if (value <= w)
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        public static int WeightedIndex<T>(this IEnumerable<T> source, float value, Func<T, float> weightSelector)
        {
            return source
                .Select(x => weightSelector.Invoke(x))
                .WeightedIndex(value);
        }

        public static int WeightedIndex<T>(this IEnumerable<T> source, Func<T, float> weightSelector)
        {
            return WeightedIndex(source, Random.value, weightSelector);
        }

        public static int RangeWithoutExcludes(int n, params int[] excludeList)
        {
            Array.Sort(excludeList);
            var valid_count = n + 1 - excludeList.Length;
            var random_index = Random.Range(0, valid_count - 1);
            var result = random_index;
            foreach (var excluded_value in excludeList)
            {
                if (result >= excluded_value)
                    result += 1;
            }

            return result;
        }

        public static RectInt? GetRandomInnerRect(this IEnumerable<Vector2Int> positions, Vector2Int size)
        {
            var shuffledPositions = positions.Shuffled();
            foreach (var position in shuffledPositions)
            {
                var rect = new RectInt(position, size);
                if (rect.RectRange().All(position => positions.Contains(position)))
                {
                    return rect;
                }
            }

            return null;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                var tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        public static IEnumerable<T> Shuffled<T>(this IEnumerable<T> ie)
        {
            var list = ie.ToList();
            var n = list.Count;

            for (var i = n - 1; i > 0; i--)
            {
                var k = Random.Range(0, i + 1);
                var value = list[k];
                list[k] = list[i];
                list[i] = value;
                yield return value;
            }
        }
    }
}