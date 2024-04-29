using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Model.Map.Tests
{
    internal class MapTest
    {
        static IEnumerable<TestCaseData> IndexTestCases
        {
            get
            {
                yield return new TestCaseData(10, 10, new Vector2Int(0, 0));
                yield return new TestCaseData(10, 10, new Vector2Int(9, 0));
                yield return new TestCaseData(10, 10, new Vector2Int(9, 9));
            }
        }
        [TestCaseSource(nameof(IndexTestCases))]
        public void IndexTest1(int width, int height, Vector2Int position)
        {
            Map map = new Map(width, height);
            Assert.AreEqual(new TileData(TileType.Blank), map.Get(position));
        }
        static IEnumerable<TestCaseData> IndexTest2Cases
        {
            get
            {
                yield return new TestCaseData(10, 10, new Vector2Int(9, 10));
            }
        }
        [TestCaseSource(nameof(IndexTest2Cases))]
        public void IndexTest2(int width, int height, Vector2Int position)
        {
            Map map = new Map(width, height);
            Assert.That(() => map.Get(position), Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
