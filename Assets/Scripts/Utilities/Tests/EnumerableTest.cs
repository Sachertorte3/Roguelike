using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
}