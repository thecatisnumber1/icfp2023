using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ICFP2023.ProblemSpec;

namespace ICFP2023
{
    public class Scorer
    {
        private readonly ProblemSpec problemSpec;

        public Scorer(ProblemSpec problemSpec)
        {
            this.problemSpec = problemSpec;
        }

        public long ComputeScore()
        {
            long score = 0;
            foreach (var musician in problemSpec.Musicians)
            {
                foreach (var attendee in problemSpec.Attendees)
                {
                    // Determine if blocked
                    bool blocked = false;
                    foreach (var blockingMusician in problemSpec.Musicians)
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

                    score += (long)Math.Ceiling(1000000 * attendee.Tastes[musician.Instrument] / attendee.Location.DistSq(musician.Location));
                }
            }

            return score;
        }

        private bool IsMusicianBlocked(Point attendee, Musician musician, Musician blockingMusician)
        {
            const float blockRadius = 5.0f;

            // Calculate the vectors and the scalar projections
            var da = attendee - musician.Location;
            var db = blockingMusician.Location - musician.Location;
            var dot = VectorDotProduct(da, db);
            var len_sq = VectorDotProduct(db, db);

            // Find the point on the line (musician -> attendee) that is closest to the blocking musician
            var t = Math.Max(0, Math.Min(len_sq, dot)) / (len_sq == 0 ? 1 : len_sq);
            var projection = musician.Location + new Vec(t * db.X, t * db.Y);

            // If this point is within the blocking radius, the musician is blocked
            var dp = blockingMusician.Location - projection;
            return VectorDotProduct(dp, dp) <= blockRadius * blockRadius;
        }

        private float VectorDotProduct(Vec a, Vec b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

    }
}
