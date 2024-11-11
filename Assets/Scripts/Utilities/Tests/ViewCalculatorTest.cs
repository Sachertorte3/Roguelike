using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using Random = System.Random;

namespace Utilities.Tests
{
    internal class ViewCalculatorTest
    {
        [Test]
        public void TestMutualVisibilityOnComplexMap()
        {
            var mapSize = 20; // 20x20 map
            var passables = GenerateComplexMap(mapSize, 0.8); // 80% passability

            PrintMap(passables, mapSize);

            foreach (var pointA in passables)
            {
                foreach (var pointB in passables)
                {
                    if (pointA != pointB)
                    {
                        Debug.Log($"Checking visibility between {pointA} and {pointB}");
                        var visibilityFromAToB = ViewCalculator.FieldOfView(pointA, new Vector2Int(mapSize, mapSize),
                            pos => passables.Contains(pos)).Contains(pointB);
                        var visibilityFromBToA = ViewCalculator.FieldOfView(pointB, new Vector2Int(mapSize, mapSize),
                            pos => passables.Contains(pos)).Contains(pointA);
                        Debug.Log(
                            $"visibilityFromAToB: {visibilityFromAToB}, visibilityFromBToA: {visibilityFromBToA}");
                        Assert.IsTrue(visibilityFromAToB == visibilityFromBToA,
                            $"Visibility should be mutual between {pointA} and {pointB}");
                    }
                }
            }
        }

        private HashSet<Vector2Int> GenerateComplexMap(int size, double passability)
        {
            var passables = new HashSet<Vector2Int>();
            var rand = new Random();

            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
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
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
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