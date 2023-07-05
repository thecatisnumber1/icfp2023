using Lib.Draw;
using static Lib.LocalFile;

namespace LibTests
{
    [TestClass]
    public class LocalFileTest
    {
        private const string TestDir = "Test Files";
        private const string UnknownDir = "ICFP Template Test Unknown";

        [TestMethod]
        public void FileCountTest()
        {
            Assert.ThrowsException<Exception>(() => FileCount(UnknownDir));

            int count = FileCount(TestDir);
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void LoadTextFileTest()
        {
            Assert.ThrowsException<Exception>(() => LoadTextFile(UnknownDir, "Text.txt"));
            Assert.ThrowsException<Exception>(() => LoadTextFile(TestDir, "Unknown Text.txt"));

            var content = LoadTextFile(TestDir, "Text.txt");
            Assert.AreEqual("Test Files Text", content);
        }

        [TestMethod]
        public void LoadJsonFileTest()
        {
            Assert.ThrowsException<Exception>(() => LoadJsonFile<Box>(UnknownDir, "Json.txt"));
            Assert.ThrowsException<Exception>(() => LoadJsonFile<Box>(TestDir, "Unknown Json.txt"));

            var content = LoadJsonFile<Box>(TestDir, "Json.txt");
            Assert.AreEqual(new Box(1, 2, 3, 4), content);
        }

        [TestMethod]
        public void LoadPNGFileTest()
        {
            Assert.ThrowsException<Exception>(() => LoadPNGFile(UnknownDir, "PNG.png"));
            Assert.ThrowsException<Exception>(() => LoadPNGFile(TestDir, "Unknown PNG.png"));

            var content = LoadPNGFile(TestDir, "PNG.png");
            Assert.AreEqual(10, content.Width);
            Assert.AreEqual(10, content.Height);
            Assert.AreEqual(new Color(34, 177, 76, 255), content[0, 0]);
        }
    }
}
