using Lib.Draw;
using System.Text.Json;

namespace LibTests.Draw
{
    [TestClass]
    public class BoxTest
    {
        [TestMethod]
        public void Construction()
        {
            var b1 = new Box(0, 0, 0, 0);
            var b2 = new Box(new Point(), new Point());
            var b3 = JsonSerializer.Deserialize<Box>("{ \"BottomLeft\": { \"X\": 0, \"Y\": 0 }, \"TopRight\": { \"X\": 0, \"Y\": 0 } }");

            Assert.AreEqual(b1, b2);
            Assert.AreEqual(b1, b3);

            var b4 = new Box(1, 2, 3, 4);
            var b5 = new Box(new Point(1, 2), new Point(3, 4));
            var b6 = JsonSerializer.Deserialize<Box>("{ \"BottomLeft\": { \"X\": 1, \"Y\": 2 }, \"TopRight\": { \"X\": 3, \"Y\": 4 } }");
            
            Assert.AreNotEqual(b1, b4);
            Assert.AreEqual(b4, b5);
            Assert.AreEqual(b4, b6);
        }

        [TestMethod]
        public void Shift()
        {
            var b = new Box(1, 2, 3, 4);
            var shift = b.Shift(1, 2);
            Assert.AreNotSame(b, shift);
            Assert.AreEqual(new Box(2, 4, 4, 6), shift);
        }

        [TestMethod]
        public void String()
        {
            var b = new Box(1, 2, 3, 4);
            var str = b.ToString();
            Assert.AreEqual("([1, 2], [3, 4])", str);
        }
    }
}
