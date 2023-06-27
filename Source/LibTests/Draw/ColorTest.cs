using Lib.Draw;
using System.Text.Json;

namespace LibTests.Draw
{
    [TestClass]
    public class ColorTest
    {
        [TestMethod]
        public void Construction()
        {
            var c1 = new Color();
            var c2 = new Color(0, 0, 0, 0);
            var c3 = JsonSerializer.Deserialize<Color>("{ \"R\": 0, \"G\": 0, \"B\": 0, \"A\": 0 }");

            Assert.AreEqual(Color.EMPTY, c1);
            Assert.AreEqual(Color.EMPTY, c2);
            Assert.AreEqual(Color.EMPTY, c3);

            var c4 = new Color(1, 2, 3, 4);
            var c5 = JsonSerializer.Deserialize<Color>("{ \"R\": 1, \"G\": 2, \"B\": 3, \"A\": 4 }");

            Assert.AreNotEqual(Color.EMPTY, c4);
            Assert.AreNotEqual(Color.EMPTY, c5);
            Assert.AreEqual(c4, c5);
        }

        [TestMethod]
        public void Clone()
        {
            var c = new Color(1, 2, 3, 4);
            var clone = c.Clone();
            Assert.AreEqual(c, clone);
            Assert.AreNotSame(c, clone);
        }

        [TestMethod]
        public void Diff()
        {
            var c1 = new Color(1, 2, 3, 4);
            var c2 = new Color(5, 6, 7, 8);
            var diff = c1.Diff(c2);
            Assert.AreEqual(8, diff);
        }

        [TestMethod]
        public void String()
        {
            var c = new Color(1, 2, 3, 4);
            var str = c.ToString();
            Assert.AreEqual("[1, 2, 3, 4]", str);
        }
    }
}
