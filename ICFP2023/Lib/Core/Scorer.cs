using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class Scorer
    {
        public static long ComputeScore(Solution solution, int[] volumes = null)
        {
            if (!solution.IsValid())
            {
                return 0;
            }

            long score = 0;

            foreach (var musician in solution.Problem.Musicians)
            {
                Point musicianLoc = solution.GetPlacement(musician);
                double q = 1;

                if (solution.Problem.UsePlayingTogetherScoring)
                {
                    foreach (var otherMusician in solution.Problem.Musicians)
                    {
                        if (!musician.Equals(otherMusician) && musician.Instrument == otherMusician.Instrument)
                        {
                            q += 1 / musicianLoc.Dist(solution.GetPlacement(otherMusician));
                        }
                    }
                }

                long singleScore = 0;

                foreach (var attendee in solution.Problem.Attendees)
                {
                    if (!IsBlocked(solution, attendee, musician))
                    {
                        double taste = attendee.Tastes[musician.Instrument];
                        double distSq = attendee.Location.DistSq(musicianLoc);
                        // Assume the volume is 10
                        singleScore += (long)Math.Ceiling(10 * q * Math.Ceiling(1000000 * taste / distSq));
                    }
                }

                // Set volume to 0 for any negative score
                if (singleScore >= 0)
                {
                    score += singleScore;

                    if (volumes != null) {
                        volumes[musician.Index] = 10;
                    }
                }
                else if (volumes != null)
                {
                    volumes[musician.Index] = 0;
                }
            }

            return score;
        }

        public static bool IsBlocked(Solution solution, Attendee attendee, Musician musician)
        {
            Point musicianLoc = solution.GetPlacement(musician);

            foreach (var blockingMusician in solution.Problem.Musicians)
            {
                Point blockingMusicianLoc = solution.GetPlacement(blockingMusician);

                if (!musician.Equals(blockingMusician) &&
                    solution.IsMusicianBlocked(attendee.Location, musicianLoc, blockingMusicianLoc))
                {
                    return true;
                }
            }

            foreach (var pillar in solution.Problem.Pillars)
            {
                if (solution.IsMusicianBlocked(attendee.Location, musician, pillar))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
