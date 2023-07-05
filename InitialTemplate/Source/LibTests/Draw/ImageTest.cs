using Lib.Draw;

namespace LibTests.Draw
{
    [TestClass]
    public class ImageTest
    {
        private readonly Color[,] testPixels = new Color[,] { 
            { new Color(1, 2, 3, 4), new Color(5, 6, 7, 8) }
        };

        [TestMethod]
        public void Construction()
        {
            var i1 = new Image(0, 0);
            var i2 = new Image(new Color[0,0]);

            Assert.AreEqual(0, i1.Width);
            Assert.AreEqual(0, i1.Height);
            Assert.AreEqual(0, i2.Width);
            Assert.AreEqual(0, i2.Height);

            var i3 = new Image(1, 2);
            var i4 = new Image(testPixels);

            Assert.AreEqual(1, i3.Width);
            Assert.AreEqual(2, i3.Height);
            Assert.AreEqual(i3[0, 0], new Color());
            Assert.AreEqual(i3[new Point()], new Color());
            Assert.AreEqual(1, i4.Width);
            Assert.AreEqual(2, i4.Height);
            Assert.AreEqual(i4[0, 0], new Color(1, 2, 3, 4));
            Assert.AreEqual(i4[new Point()], new Color(1, 2, 3, 4));
        }

        [TestMethod]
        public void Clone()
        {
            var i = new Image(testPixels);
            var clone = i.Clone();
            Assert.AreEqual(i[0, 0], clone[0, 0]);
            Assert.AreNotSame(i, clone);
        }

        [TestMethod]
        public void Extract()
        {
            var i = new Image(testPixels);
            var extract = i.Extract(new Point(0, 0), new Point(1, 1));
            Assert.AreEqual(i[0, 0], extract[0, 0]);
            Assert.AreEqual(i.Width, extract.Width);
            Assert.AreNotEqual(i.Height, extract.Height);
            Assert.AreNotSame(i, extract);

        }
    }
}
