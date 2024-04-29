using NUnit.Framework;
using Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utilities.Tests
{
    internal class EnumerableTest
    {
        static RectInt rect1 = new RectInt(0, 0, 3, 3);
        static RectInt rect2 = new RectInt(1, 2, 3, 5);
        static RectInt rect3 = new RectInt(1, 2, 10, 0);

        static IEnumerable<TestCaseData> CountTestCases
        {
            get
            {
                yield return new TestCaseData(rect1, 9);
                yield return new TestCaseData(rect2, 15);
                yield return new TestCaseData(rect3, 0);
            }
        }
        [TestCaseSource(nameof(CountTestCases))]
        public void CountTest1(RectInt rect, int expectedCount)
        {
            Assert.AreEqual(expectedCount, rect.RectRange().Count());
        }
        static IEnumerable<TestCaseData> EnumerateTestCases
        {
            get
            {
                yield return new TestCaseData(rect1, new HashSet<Vector2Int>
                {
                    new Vector2Int(0,0),
                    new Vector2Int(0,1),
                    new Vector2Int(0,2),
                    new Vector2Int(1,0),
                    new Vector2Int(1,1),
                    new Vector2Int(1,2),
                    new Vector2Int(2,0),
                    new Vector2Int(2,1),
                    new Vector2Int(2,2)
                });
            }
        }
        [TestCaseSource(nameof(EnumerateTestCases))]
        public void EnumerateTest1(RectInt rect, IEnumerable<Vector2Int> expected)
        {
            IEnumerable<Vector2Int> set = expected.Except(rect.RectRange());
            Assert.AreEqual(0, set.Count());
        }
    }
}
