using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Utilities.Tests
{
    internal class DirectionTest
    {
        private static IEnumerable<Direction8> Directions = DirectionMethods.AllDirections;

        [TestCaseSource(nameof(Directions))]
        public void AngleTest1(Direction8 direction)
        {
            Assert.AreEqual(direction, Direction8.Right.RotateAntiClockwise(direction.Angle()));
        }
        [TestCaseSource(nameof(Directions))]
        public void AngleTest2(Direction8 direction)
        {
            Assert.AreEqual(new Angle(45), direction.Rotate45AntiClockwise().Angle() - direction.Angle());
        }
        [TestCaseSource(nameof(Directions))]
        public void RotateTest1(Direction8 direction)
        {
            Assert.AreEqual(direction, direction.Rotate45Clockwise().Rotate45AntiClockwise());
        }
        [TestCaseSource(nameof(Directions))]
        public void RotateTest2(Direction8 direction)
        {
            Assert.AreEqual(direction.Rotate90Clockwise(), direction.Rotate45Clockwise().Rotate45Clockwise());
        }
        [TestCaseSource(nameof(Directions))]
        public void RotateTest3(Direction8 direction)
        {
            Assert.AreEqual(direction.Reverse(), direction.Rotate90Clockwise().Rotate90Clockwise());
        }
        [TestCaseSource(nameof(Directions))]
        public void VectorTest(Direction8 direction)
        {
            Assert.AreEqual(direction.Angle(), new Angle(Vector2.SignedAngle(Vector2.right, direction.Vector())));
        }
        [TestCaseSource(nameof(Directions))]
        public void VectorTest2(Direction8 direction)
        {
            Assert.AreEqual(direction, DirectionMethods.FromVector(direction.Vector()));
        }
    }
}