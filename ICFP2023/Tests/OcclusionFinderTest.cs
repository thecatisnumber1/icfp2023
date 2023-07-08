namespace Tests
{
    [TestClass]
    public class OcclusionFinderTest
    {
        [TestMethod]
        public void TestOcclusionMusicianBlocking()
        {
            var prob = ProblemSpec.ReadJson(ProblemSpecTest.PROBLEM);
            var solution = new Solution(prob);
            solution.SetPlacement(prob.Musicians[0], new(590.0, 10.0));
            solution.SetPlacement(prob.Musicians[1], new(1100.0, 100.0));
            solution.SetPlacement(prob.Musicians[2], new(1100.0, 150.0));

            var scorer = new OcclusionFinder(solution);
            scorer.OnPlacementChanged(prob.Musicians[0], Point.ORIGIN);
            scorer.OnPlacementChanged(prob.Musicians[1], Point.ORIGIN);
            scorer.OnPlacementChanged(prob.Musicians[2], Point.ORIGIN);

            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[0], prob.Attendees[0]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[0], prob.Attendees[1]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[0], prob.Attendees[2]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[1], prob.Attendees[0]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[1], prob.Attendees[1]));
            Assert.IsTrue(scorer.IsMusicianBlocked(prob.Musicians[1], prob.Attendees[2]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[2], prob.Attendees[0]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[2], prob.Attendees[1]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[2], prob.Attendees[2]));
        }

        [TestMethod]
        public void TestOcclusionPillarBlocking()
        {
            // The sample problem has a pillar that doesn't block anyone.
            // Add one that blocks musician 0 from attendee 0.
            Pillar pillar = new(new(100.0, 300.0), 21.0);
            var prob = ProblemSpec.ReadJson(ProblemSpecTest.PROBLEM);
            prob.Pillars.Add(pillar);

            var solution = new Solution(prob);
            solution.SetPlacement(prob.Musicians[0], new(100.0, 10.0));
            solution.SetPlacement(prob.Musicians[1], new(1100.0, 100.0));
            solution.SetPlacement(prob.Musicians[2], new(1100.0, 150.0));

            var scorer = new OcclusionFinder(solution);
            scorer.OnPlacementChanged(prob.Musicians[0], Point.ORIGIN);
            scorer.OnPlacementChanged(prob.Musicians[1], Point.ORIGIN);
            scorer.OnPlacementChanged(prob.Musicians[2], Point.ORIGIN);

            Assert.IsTrue(scorer.IsMusicianBlocked(prob.Musicians[0], prob.Attendees[0]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[0], prob.Attendees[1]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[0], prob.Attendees[2]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[1], prob.Attendees[0]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[1], prob.Attendees[1]));
            Assert.IsTrue(scorer.IsMusicianBlocked(prob.Musicians[1], prob.Attendees[2]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[2], prob.Attendees[0]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[2], prob.Attendees[1]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[2], prob.Attendees[2]));
        }

        [TestMethod]
        public void TestOcclusionPillarBlockingEdge()
        {
            // Add a pillar where the line intersects the pillar at one point,
            // which should NOT block.
            Pillar pillar = new(new(79.0, 300.0), 21.0);
            var prob = ProblemSpec.ReadJson(ProblemSpecTest.PROBLEM);
            prob.Pillars.Add(pillar);

            var solution = new Solution(prob);
            solution.SetPlacement(prob.Musicians[0], new(100.0, 10.0));
            solution.SetPlacement(prob.Musicians[1], new(1100.0, 100.0));
            solution.SetPlacement(prob.Musicians[2], new(1100.0, 150.0));

            var scorer = new OcclusionFinder(solution);
            scorer.OnPlacementChanged(prob.Musicians[0], Point.ORIGIN);
            scorer.OnPlacementChanged(prob.Musicians[1], Point.ORIGIN);
            scorer.OnPlacementChanged(prob.Musicians[2], Point.ORIGIN);

            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[0], prob.Attendees[0]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[0], prob.Attendees[1]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[0], prob.Attendees[2]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[1], prob.Attendees[0]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[1], prob.Attendees[1]));
            Assert.IsTrue(scorer.IsMusicianBlocked(prob.Musicians[1], prob.Attendees[2]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[2], prob.Attendees[0]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[2], prob.Attendees[1]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[2], prob.Attendees[2]));
        }

        [TestMethod]
        public void TestOcclusionPillarBlockingAlmostEdge()
        {
            // Add a pillar where the line intersects almost at the edge of
            // the pillar, but still at more than one point.
            Pillar pillar = new(new(79.1, 300.0), 21.0);
            var prob = ProblemSpec.ReadJson(ProblemSpecTest.PROBLEM);
            prob.Pillars.Add(pillar);

            var solution = new Solution(prob);
            solution.SetPlacement(prob.Musicians[0], new(100.0, 10.0));
            solution.SetPlacement(prob.Musicians[1], new(1100.0, 100.0));
            solution.SetPlacement(prob.Musicians[2], new(1100.0, 150.0));

            var scorer = new OcclusionFinder(solution);
            scorer.OnPlacementChanged(prob.Musicians[0], Point.ORIGIN);
            scorer.OnPlacementChanged(prob.Musicians[1], Point.ORIGIN);
            scorer.OnPlacementChanged(prob.Musicians[2], Point.ORIGIN);

            Assert.IsTrue(scorer.IsMusicianBlocked(prob.Musicians[0], prob.Attendees[0]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[0], prob.Attendees[1]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[0], prob.Attendees[2]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[1], prob.Attendees[0]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[1], prob.Attendees[1]));
            Assert.IsTrue(scorer.IsMusicianBlocked(prob.Musicians[1], prob.Attendees[2]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[2], prob.Attendees[0]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[2], prob.Attendees[1]));
            Assert.IsFalse(scorer.IsMusicianBlocked(prob.Musicians[2], prob.Attendees[2]));
        }

        [TestMethod]
        public void TestOcclusionPillarOnEdge()
        {
            // Add a pillar on the edge of the room
            Pillar pillar = new(new(0, 300.0), 21.0);
            var prob = ProblemSpec.ReadJson(ProblemSpecTest.PROBLEM);
            prob.Pillars.Add(pillar);

            new Solution(prob);
        }
    }
}
