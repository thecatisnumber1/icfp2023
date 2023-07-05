using Lib;
using Lib.Draw;

namespace LibTests
{
    [TestClass]
    public class CacheTest
    {
        [TestMethod]
        public void CacheInt()
        {
            int callCount = 0;
            var func = (int t) => {
                callCount++;
                return t * t;
            };

            var cache = new Cache<int, int>(func);
            
            Assert.AreEqual(0, callCount);

            void validate(bool expectFromCache)
            {
                int priorCallCount = callCount;
                var v1 = cache.Get(2);
                var v2 = cache.Get(3);

                Assert.AreEqual(priorCallCount + (expectFromCache ? 0 : 2), callCount);
                Assert.AreEqual(4, v1);
                Assert.AreEqual(9, v2);
            }

            validate(expectFromCache: false);
            validate(expectFromCache: true);

            cache.Clear();

            validate(expectFromCache: false);
        }

        [TestMethod]
        public void CachePoint()
        {
            int callCount = 0;
            var func = (Point p) => {
                callCount++;
                return p.X + p.Y;
            };

            var cache = new Cache<Point, int>(func);

            Assert.AreEqual(0, callCount);

            void validate(bool expectFromCache)
            {
                int priorCallCount = callCount;
                var v1 = cache.Get(new Point(1, 2));
                var v2 = cache.Get(new Point(2, 3));

                Assert.AreEqual(priorCallCount + (expectFromCache ? 0 : 2), callCount);
                Assert.AreEqual(3, v1);
                Assert.AreEqual(5, v2);
            }

            validate(expectFromCache: false);
            validate(expectFromCache: true);

            cache.Clear();

            validate(expectFromCache: false);
        }

        [TestMethod]
        public void CacheList()
        {
            int callCount = 0;
            var func = (List<int> list) => {
                callCount++;
                return list.Sum();
            };

            var cache = new Cache<List<int>, int>(func);

            Assert.AreEqual(0, callCount);

            void validate(bool expectFromCache)
            {
                int priorCallCount = callCount;
                var v1 = cache.Get(new List<int>() { 1, 2, 3 });
                var v2 = cache.Get(new List<int>() { 3, 2, 1 });

                Assert.AreEqual(priorCallCount + (expectFromCache ? 0 : 2), callCount);
                Assert.AreEqual(6, v1);
                Assert.AreEqual(6, v2);
            }

            validate(expectFromCache: false);
            validate(expectFromCache: true);

            cache.Clear();

            validate(expectFromCache: false);
        }

        [TestMethod]
        public void CacheDepth2()
        {
            int callCount = 0;
            var func = (int x, Point p) =>
            {
                callCount++;
                return x * p.X;
            };

            var cache = new Cache<int, Point, int>(func);

            Assert.AreEqual(0, callCount);

            void validate(bool expectFromCache)
            {
                int priorCallCount = callCount;
                var v1 = cache.Get(2, new Point(1, 2));
                var v2 = cache.Get(2, new Point(2, 3));
                var v3 = cache.Get(3, new Point(1, 2));

                Assert.AreEqual(priorCallCount + (expectFromCache ? 0 : 3), callCount);
                Assert.AreEqual(2, v1);
                Assert.AreEqual(4, v2);
                Assert.AreEqual(3, v3);
            }

            validate(expectFromCache: false);
            validate(expectFromCache: true);

            cache.Clear();

            validate(expectFromCache: false);
        }

        [TestMethod]
        public void CacheDepth3()
        {
            int callCount = 0;
            var func = (int x, Point p, List<int> list) =>
            {
                callCount++;
                return x * p.X * list.Sum();
            };

            var cache = new Cache<int, Point, List<int>, int>(func);

            Assert.AreEqual(0, callCount);

            void validate(bool expectFromCache)
            {
                int priorCallCount = callCount;
                var v1 = cache.Get(2, new Point(1, 2), new List<int> { 1, 2, 3 });
                var v2 = cache.Get(2, new Point(2, 3), new List<int> { 1, 2, 3 });
                var v3 = cache.Get(2, new Point(1, 2), new List<int> { 3, 2, 1 });
                var v4 = cache.Get(2, new Point(2, 3), new List<int> { 3, 2, 1 });
                var v5 = cache.Get(3, new Point(1, 2), new List<int> { 1, 2, 3 });
                var v6 = cache.Get(3, new Point(1, 2), new List<int> { 3, 2, 1 });
                var v7 = cache.Get(3, new Point(2, 3), new List<int> { 1, 2, 3 });
                var v8 = cache.Get(3, new Point(2, 3), new List<int> { 3, 2, 1 });

                Assert.AreEqual(priorCallCount + (expectFromCache ? 0 : 8), callCount);
                Assert.AreEqual(12, v1);
                Assert.AreEqual(24, v2);
                Assert.AreEqual(12, v3);
                Assert.AreEqual(24, v4);
                Assert.AreEqual(18, v5);
                Assert.AreEqual(18, v6);
                Assert.AreEqual(36, v7);
                Assert.AreEqual(36, v8);
            }

            validate(expectFromCache: false);
            validate(expectFromCache: true);

            cache.Clear();

            validate(expectFromCache: false);
        }
    }
}
