namespace Tests
{
    [TestClass]
    public class ProblemSpecTest
    {
        const string PROBLEM = @"{
            ""room_width"" : 2000.0,
            ""room_height"" : 5000.0,
            ""stage_width"" : 1000.0,
            ""stage_height"" : 200.0,
            ""stage_bottom_left"" : [500.0 , 0.0],
            ""musicians"" : [0, 1, 0],
            ""attendees"" : [
                { ""x"": 100.0, ""y"": 500.0, ""tastes"": [1000.0, -1000.0] },
                { ""x"": 200.0, ""y"": 1000.0, ""tastes"": [200.0, 200.0] }
            ]
        }";


        [TestMethod]
        public void TestReadJson()
        {
            var spec = ProblemSpec.ReadJson(PROBLEM);
        }
    }
}