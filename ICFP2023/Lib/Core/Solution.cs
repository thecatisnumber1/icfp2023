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

        public long ScoreCache { get; private set; }

        // Index of musician to the list of scores for each attendee
        private Dictionary<int, List<long>> MusicianScoreCache;

        // Index of the musician to the list of attendees that are blocked from that perspective
        private Dictionary<Point, HashSet<int>> MusicianBlockedCache;

        public Solution(ProblemSpec problem)
        {
            this.Problem = problem;
            Placements = new List<Point>();
            for (int i = 0; i < problem.Musicians.Count; i++)
            {
                Placements.Add(Point.ORIGIN);
            }
        }

        private Solution(ProblemSpec problem, List<Point> placements, Dictionary<int, List<long>> musicianScoreCache, Dictionary<Point, HashSet<int>> musicianBlockedCache, long scoreCache)
        {
            Problem = problem;
            Placements = placements;
            MusicianScoreCache = musicianScoreCache;
            MusicianBlockedCache = musicianBlockedCache;
            ScoreCache = scoreCache;
        }

        public Point GetPlacement(Musician musician)
        {
            return Placements[musician.Index];
        }

        public void SetPlacement(Musician musician, Point loc)
        {
            Placements[musician.Index] = loc;
        }

        public void Swap(int m0, int m1)
        {
            for (int i = 0; i < Problem.Attendees.Count; i++)
            {
                ScoreCache -= MusicianScoreCache[m0][i];
                ScoreCache -= MusicianScoreCache[m1][i];
            }

            var temp = Placements[m0];
            Placements[m0] = Placements[m1];
            Placements[m1] = temp;

            for (int i = 0; i < Problem.Attendees.Count; i++)
            {
                MusicianScoreCache[m0][i] = 0;
                MusicianScoreCache[m1][i] = 0;
                if (!MusicianBlockedCache[Placements[m0]].Contains(i))
                {
                    MusicianScoreCache[m0][i] = PairScore(m0, i); ;
                }

                if (!MusicianBlockedCache[Placements[m1]].Contains(i))
                {
                    MusicianScoreCache[m1][i] = PairScore(m1, i); ;
                }

                ScoreCache += MusicianScoreCache[m0][i];
                ScoreCache += MusicianScoreCache[m1][i];
            }
        }

        public Solution Copy()
        {
            Dictionary<int, List<long>> cacheCopy = new Dictionary<int, List<long>>();
            foreach (var kvp in MusicianScoreCache)
            {
                cacheCopy.Add(kvp.Key, new List<long>(kvp.Value));
            }

            return new Solution(Problem, new List<Point>(Placements), cacheCopy, MusicianBlockedCache, ScoreCache);
        }

        private void ResetCaches()
        {
            MusicianScoreCache = new Dictionary<int, List<long>>();
            MusicianBlockedCache = new Dictionary<Point, HashSet<int>>();
            for (int i = 0; i < Problem.Musicians.Count; i++)
            {
                MusicianScoreCache.Add(i, new List<long>());
                MusicianBlockedCache.Add(Placements[i], new HashSet<int>());
                for (int j = 0; j < Problem.Attendees.Count; j++)
                {
                    MusicianScoreCache[i].Add(0);
                }
            }
        }

        public long InitializeScore()
        {
            ScoreCache = 0;
            ResetCaches();
            for (int musicianIndex = 0; musicianIndex < Problem.Musicians.Count; musicianIndex++)
            {
                var musician = Problem.Musicians[musicianIndex];
                for (int attendeeIndex = 0; attendeeIndex < Problem.Attendees.Count; attendeeIndex++)
                {
                    var attendee = Problem.Attendees[attendeeIndex];

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
                            MusicianBlockedCache[Placements[musicianIndex]].Add(attendeeIndex);
                            break;
                        }
                    }

                    if (blocked)
                    {
                        continue;
                    }

                    long tempScore = PairScore(musicianIndex, attendeeIndex);
                    MusicianScoreCache[musicianIndex][attendeeIndex] = tempScore;
                    ScoreCache += tempScore;
                }
            }

            return ScoreCache;
        }

        // Assumes no blocking!
        private long PairScore(int musicianIndex, int attendeeIndex)
        {
            return (long)Math.Ceiling(1000000 * Problem.Attendees[attendeeIndex].Tastes[Problem.Musicians[musicianIndex].Instrument] /
                Problem.Attendees[attendeeIndex].Location.DistSq(Placements[musicianIndex]));
        }

        public bool IsValid()
        {
            // Make sure the musicians won't fall off the stage
            foreach(var point in Placements)
            {
                double minX = Problem.StageBottomLeft.X + Musician.SOCIAL_DISTANCE;
                double maxX = Problem.StageBottomLeft.X + Problem.StageWidth - Musician.SOCIAL_DISTANCE;
                double minY = Problem.StageBottomLeft.Y + Musician.SOCIAL_DISTANCE;
                double maxY = Problem.StageBottomLeft.Y + Problem.StageHeight - Musician.SOCIAL_DISTANCE;

                if (point == Point.ORIGIN ||
                    point.X < minX || point.X > maxX ||
                    point.Y < minY || point.Y > maxY)
                {
                    return false;
                }
            }

            // Check for musician social distancing
            for (int i = 0; i < Placements.Count; i++)
            {
                for (int j = i + 1; j < Placements.Count; j++)
                {
                    if (Placements[i].DistSq(Placements[j]) < Musician.SOCIAL_DISTANCE * Musician.SOCIAL_DISTANCE)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IsMusicianBlocked(Point attendee, Musician musician, Musician blockingMusician)
        {
            var musicianLoc = GetPlacement(musician);
            var blockingLoc = GetPlacement(blockingMusician);

            // Calculate the vectors
            var da = attendee - musicianLoc; // vector from musician to attendee
            var db = blockingLoc - musicianLoc; // vector from musician to blockingMusician

            // Calculate the dot product and magnitude squared
            var dot = da.DotProduct(db);
            var len_sq = da.DotProduct(da); // magnitude squared of da

            // Compute t as the scalar projection without clamping
            var t = dot / (len_sq == 0 ? 1 : len_sq);

            // The point on the line (musician to attendee) closest to the blocking musician
            Point projection;

            // If t is less than 0, the projection falls before the musician's position. If t is greater than 1, it falls after the attendee's position.
            if (t < 0)
            {
                projection = musicianLoc;
            }
            else if (t > 1)
            {
                projection = attendee;
            }
            else
            {
                // Compute the projection of the point on the line from the musician to the attendee
                projection = musicianLoc + t * da; // note: da is the vector from musician to attendee
            }

            // If this point is within the blocking radius, the musician is blocked
            var dp = blockingLoc - projection;
            return dp.DotProduct(dp) <= Musician.BLOCKING_RADIUS * Musician.BLOCKING_RADIUS;
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
