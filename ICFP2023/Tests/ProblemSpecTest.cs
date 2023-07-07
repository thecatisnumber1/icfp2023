namespace Tests
{
    [TestClass]
    public class ProblemSpecTest
    {
        public const string PROBLEM = @"{
            ""room_width"" : 2000.0,
            ""room_height"" : 5000.0,
            ""stage_width"" : 1000.0,
            ""stage_height"" : 200.0,
            ""stage_bottom_left"" : [500.0 , 0.0],
            ""musicians"" : [0, 1, 0],
            ""attendees"" : [
                { ""x"": 100.0, ""y"": 500.0, ""tastes"": [1000.0, -1000.0] },
                { ""x"": 200.0, ""y"": 1000.0, ""tastes"": [200.0, 200.0] },
                { ""x"": 1100.0, ""y"": 800.0, ""tastes"": [800.0, 1500.0] }
            ]
        }";

        const string SOLUTION = @"{
            ""placements"" : [
                { ""x"" : 590.0 , ""y"" : 10.0 },
                { ""x"" : 1100.0 , ""y"" : 100.0 },
                { ""x"" : 1100.0 , ""y"" : 150.0 }
            ]
        }";

        [TestMethod]
        public void TestReadJson()
        {
            var spec = ProblemSpec.ReadJson(PROBLEM);
            Assert.IsNotNull(spec);
            Assert.AreEqual(2000.0f, spec.RoomWidth);
            Assert.AreEqual(5000.0f, spec.RoomHeight);
            Assert.AreEqual(1000.0f, spec.StageWidth);
            Assert.AreEqual(200.0f, spec.StageHeight);
            Assert.AreEqual(new Point(500.0f, 0.0f), spec.StageBottomLeft);
            Assert.AreEqual(new Musician(0, 0), spec.Musicians[0]);
            Assert.AreEqual(new Musician(1, 1), spec.Musicians[1]);
            Assert.AreEqual(new Musician(2, 0), spec.Musicians[2]);
            Assert.AreEqual(new Attendee(100.0f, 500.0f, new float[] { 1000.0f, -1000.0f }.ToList()), spec.Attendees[0]);
            Assert.AreEqual(new Attendee(200.0f, 1000.0f, new float[] { 200.0f, 200.0f }.ToList()), spec.Attendees[1]);
            Assert.AreEqual(new Attendee(1100.0f, 800.0f, new float[] { 800.0f, 1500.0f }.ToList()), spec.Attendees[2]);
        }

        [TestMethod]
        public void TestWriteSolution()
        {
            var solution = new Solution(ProblemSpec.ReadJson(PROBLEM));
            solution.Placements[0] = new(590.0f, 10.0f);
            solution.Placements[1] = new(1100.0f, 100.0f);
            solution.Placements[2] = new(1100.0f, 150.0f);
            string json = solution.WriteJson();

            Assert.AreEqual(SOLUTION.Replace(" ", "").Replace("\r", "").Replace("\n", ""), json);
        }
    }
}