using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace Utilities.Tests
{
    internal class EnumerableTest
    {
        private static readonly RectInt rect1 = new(0, 0, 3, 3);
        private static readonly RectInt rect2 = new(1, 2, 3, 5);
        private static readonly RectInt rect3 = new(1, 2, 10, 0);

        private static IEnumerable<TestCaseData> CountTestCases
        {
            get
            {
                yield return new TestCaseData(rect1, 9);
                yield return new TestCaseData(rect2, 15);
                yield return new TestCaseData(rect3, 0);
            }
        }

        private static IEnumerable<TestCaseData> EnumerateTestCases
        {
            get
            {
                yield return new TestCaseData(rect1, new HashSet<Vector2Int>
                {
                    new(0, 0),
                    new(0, 1),
                    new(0, 2),
                    new(1, 0),
                    new(1, 1),
                    new(1, 2),
                    new(2, 0),
                    new(2, 1),
                    new(2, 2)
                });
            }
        }

        private static IEnumerable<TestCaseData> GetRandomTest1Case
        {
            get
            {
                yield return new TestCaseData(new List<int> { 0, 2, 4, 6, 8 }, TestRandom(0), 0);
                yield return new TestCaseData(new List<int> { 0, 2, 4, 6, 8 }, TestRandom(1), 2);
                yield return new TestCaseData(new List<int> { 0, 2, 4, 6, 8 }, TestRandom(2), 4);
            }
        }

        private static IEnumerable<TestCaseData> GetRandomTest2Case
        {
            get
            {
                yield return new TestCaseData(new List<int> { 0, 2, 4, 6, 8 }, 1, TestRandom(0), new List<int> { 0 });
                yield return new TestCaseData(new List<int> { 0, 2, 4, 6, 8 }, 3, TestRandom(1),
                    new List<int> { 2, 8, 6 });
                yield return new TestCaseData(new List<int> { 0, 2, 4, 6, 8 }, 5, TestRandom(2),
                    new List<int> { 4, 8, 6, 2, 0 });
            }
        }

        [TestCaseSource(nameof(CountTestCases))]
        public void CountTest1(RectInt rect, int expectedCount)
        {
            Assert.AreEqual(expectedCount, rect.RectRange().Count());
        }

        [TestCaseSource(nameof(EnumerateTestCases))]
        public void EnumerateTest1(RectInt rect, IEnumerable<Vector2Int> expected)
        {
            var set = expected.Except(rect.RectRange());
            Assert.AreEqual(0, set.Count());
        }

        private static Func<int, int> TestRandom(int result)
        {
            return max => Math.Min(result, max - 1);
        }

        [TestCaseSource(nameof(GetRandomTest1Case))]
        public void GetRandomTest1(List<int> list, Func<int, int> random, int expected)
        {
            Assert.AreEqual(expected, list.GetAtRandom(1, random)[0]);
        }

        [TestCaseSource(nameof(GetRandomTest2Case))]
        public void GetRandomTest2(List<int> list, int n, Func<int, int> random, List<int> expected)
        {
            Assert.AreEqual(expected, list.GetAtRandom(n, random));
        }
    }
    
    internal class ViewCalculatorTest
    {

        [Test]
        public void TestMutualVisibilityOnComplexMap()
        {
            int mapSize = 20; // 20x20 map
            var passables = GenerateComplexMap(mapSize, 0.8); // 80% passability

            PrintMap(passables, mapSize);

            foreach (var pointA in passables)
            {
                foreach (var pointB in passables)
                {
                    if (pointA != pointB)
                    {
                        Debug.Log($"Checking visibility between {pointA} and {pointB}");
                        var visibilityFromAToB = ViewCalculator.FieldOfView(pointA, new Vector2Int(mapSize, mapSize), pos => passables.Contains(pos)).Contains(pointB);
                        var visibilityFromBToA = ViewCalculator.FieldOfView(pointB, new Vector2Int(mapSize, mapSize), pos => passables.Contains(pos)).Contains(pointA);
                        Debug.Log($"visibilityFromAToB: {visibilityFromAToB}, visibilityFromBToA: {visibilityFromBToA}");
                        Assert.IsTrue(visibilityFromAToB == visibilityFromBToA, $"Visibility should be mutual between {pointA} and {pointB}");
                    }
                }
            }
        }

        private HashSet<Vector2Int> GenerateComplexMap(int size, double passability)
        {
            var passables = new HashSet<Vector2Int>();
            var rand = new System.Random();

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (x == 0 || y == 0 || x == size - 1 || y == size - 1)
                    {
                        // 外周は壁なので通行不可能
                    }
                    else
                    {
                        // 内部のセルの通行可能性をランダムに設定
                        if (rand.NextDouble() < passability)
                        {
                            passables.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }
            return passables;
        }

        private void PrintMap(HashSet<Vector2Int> passables, int size)
        {
            var map = new StringBuilder();
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 通行可能なセルは '.' で、通行不可能なセルは '#' で表示
                    map.Append(passables.Contains(new Vector2Int(x, y)) ? '□' : '■');
                }
                map.AppendLine(); // 各行の後に改行を追加
            }
            Debug.Log(map.ToString()); // マップの状態をログに出力
        }
    }
}