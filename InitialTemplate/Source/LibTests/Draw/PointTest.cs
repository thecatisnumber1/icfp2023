using Lib.Draw;
using System.Text.Json;

namespace LibTests.Draw
{
    [TestClass]
    public class PointTest
    {
        [TestMethod]
        public void Construction()
        {
            var p1 = new Point();
            var p2 = new Point(0, 0);
            var p3 = JsonSerializer.Deserialize<Point>("{ \"X\": 0, \"Y\": 0 }");

            Assert.AreEqual(Point.ORIGIN, p1);
            Assert.AreEqual(Point.ORIGIN, p2);
            Assert.AreEqual(Point.ORIGIN, p3);

            var p4 = new Point(1, 2);
            var p5 = JsonSerializer.Deserialize<Point>("{ \"X\": 1, \"Y\": 2 }");

            Assert.AreNotEqual(Point.ORIGIN, p4);
            Assert.AreNotEqual(Point.ORIGIN, p5);
            Assert.AreEqual(p4, p5);
        }

        [TestMethod]
        public void Clone()
        {
            var p = new Point(1, 2);
            var clone = p.Clone();
            Assert.AreEqual(p, clone);
            Assert.AreNotSame(p, clone);
        }

        [TestMethod]
        public void Add()
        {
            var p1 = new Point(1, 2);
            var p2 = new Point(3, 4);
            var add = p1.Add(p2);
            Assert.AreEqual(new Point(4, 6), add);
        }

        [TestMethod]
        public void Subtract()
        {
            var p1 = new Point(1, 2);
            var p2 = new Point(3, 4);
            var sub = p2.Subtract(p1);
            Assert.AreEqual(new Point(2, 2), sub);
        }

        [TestMethod]
        public void Manhattan()
        {
            var p1 = new Point(1, 2);
            var p2 = new Point(3, 4);
            var dist = p1.ManhattanDist(p2);
            Assert.AreEqual(4, dist);
        }

        [TestMethod]
        public void String()
        {
            var p = new Point(1, 2);
            var str = p.ToString();
            Assert.AreEqual("[1, 2]", str);
        }
    }
}