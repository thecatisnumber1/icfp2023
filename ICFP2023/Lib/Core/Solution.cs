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
        public readonly ProblemSpec Problem;

        [JsonProperty("placements")]
        public readonly Point[] Placements;

        public Solution(ProblemSpec problem)
        {
            this.Problem = problem;
            Placements = new Point[problem.Musicians.Count];
        }

        public Point GetPlacement(Musician musician)
        {
            return Placements[musician.Index];
        }

        public void SetPlacement(Musician musician, Point loc)
        {
            Placements[musician.Index] = loc;
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

        public string WriteJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
