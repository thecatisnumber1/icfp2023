using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class Solution
    {
        [JsonIgnore]
        public ProblemSpec Problem { get; private set; }

        [JsonProperty("placements")]
        public List<Point> Placements { get; init; }

        [JsonConstructor]
        private Solution(List<Point> placements)
        {
            this.Placements = placements;
        }

        public Solution(ProblemSpec problem)
        {
            this.Problem = problem;
            Placements = new List<Point>();
            for (int i = 0; i < problem.Musicians.Count; i++)
            {
                Placements.Add(Point.ORIGIN);
            }
        }

        private Solution(ProblemSpec problem, List<Point> placements)
        {
            this.Problem = problem;
            this.Placements = placements;
        }

        public Point GetPlacement(Musician musician)
        {
            return Placements[musician.Index];
        }

        public void SetPlacement(Musician musician, Point loc)
        {
            Placements[musician.Index] = loc;
        }

        public Solution Copy()
        {
            return new Solution(Problem, Placements.ToList());
        }

        public long ComputeScore()
        {
            long score = 0;
            foreach (var musician in Problem.Musicians)
            {
                foreach (var attendee in Problem.Attendees)
                {
                    // Determine if blocked
                    bool blocked = false;
                    foreach (var blockingMusician in Problem.Musicians)
                    {
                        if (blockingMusician == musician)
                        {
                            continue;
                        }

                        if (IsMusicianBlocked(attendee.Location, musician, blockingMusician))
                        {
                            blocked = true;
                            break;
                        }
                    }

                    if (blocked)
                    {
                        continue;
                    }

                    score += (long)Math.Ceiling(1000000 * attendee.Tastes[musician.Instrument] / attendee.Location.DistSq(GetPlacement(musician)));
                }
            }

            return score;
        }

        public bool IsValid()
        {
            // TODO: Check distances to other musicians
            foreach(var point in Placements)
            {
                float minX = Problem.StageBottomLeft.X + Musician.SOCIAL_DISTANCE;
                float maxX = Problem.StageBottomLeft.X + Problem.StageWidth - Musician.SOCIAL_DISTANCE;
                float minY = Problem.StageBottomLeft.Y + Musician.SOCIAL_DISTANCE;
                float maxY = Problem.StageBottomLeft.Y + Problem.StageHeight - Musician.SOCIAL_DISTANCE;

                if (point == Point.ORIGIN ||
                    point.X < minX || point.X > maxX ||
                    point.Y < minY || point.Y > maxY)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsMusicianBlocked(Point attendee, Musician musician, Musician blockingMusician)
        {
            const float blockRadius = 5.0f;
            var musicianLoc = GetPlacement(musician);
            var blockingLoc = GetPlacement(blockingMusician);

            // Calculate the vectors and the scalar projections
            var da = attendee - musicianLoc;
            var db = blockingLoc - musicianLoc;
            var dot = da.DotProduct(db);
            var len_sq = db.DotProduct(db);

            // Find the point on the line (musician -> attendee) that is closest to the blocking musician
            var t = Math.Max(0, Math.Min(len_sq, dot)) / (len_sq == 0 ? 1 : len_sq);
            var projection = musicianLoc + new Vec(t * db.X, t * db.Y);

            // If this point is within the blocking radius, the musician is blocked
            var dp = blockingLoc - projection;
            return dp.DotProduct(dp) <= blockRadius * blockRadius;
        }

        public static Solution Read(string solutionPath, ProblemSpec problem)
        {
            var solutionJson = FileUtil.Read(solutionPath);
            var solution = ReadJson(solutionJson);
            solution.Problem = problem;
            return solution;
        }

        public static Solution ReadJson(string solutionJson)
        {
            return JsonConvert.DeserializeObject<Solution>(solutionJson);
        }

        public string WriteJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
