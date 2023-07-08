﻿using Newtonsoft.Json.Linq;
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
        public ProblemSpec Problem { get; private set; }
        public IReadOnlyList<Point> Placements => placements;
        private List<Point> placements;

        private Solution(List<Point> placements)
        {
            this.placements = placements;
        }

        public long ScoreCache { get; private set; }

        // Index of musician to the list of scores for each attendee
        private Dictionary<int, List<long>> MusicianScoreCache;

        // Position of the musician to the list of attendees that are blocked from that perspective
        private Dictionary<Point, HashSet<int>> MusicianBlockedCache;

        // Musician to q(i) for playing together
        private Dictionary<int, double> MusicianDistanceScoreCache;

        private OcclusionFinder occlusionFinder;

        public Solution(ProblemSpec problem)
        {
            this.Problem = problem;
            placements = new List<Point>();
            for (int i = 0; i < problem.Musicians.Count; i++)
            {
                placements.Add(Point.ORIGIN);
            }

            this.occlusionFinder = new(this);
        }

        private Solution(ProblemSpec problem,
            List<Point> placements,
            Dictionary<int, List<long>> musicianScoreCache,
            Dictionary<int, double> musicianDistanceScoreCache,
            Dictionary<Point, HashSet<int>> musicianBlockedCache,
            long scoreCache)
        {
            Problem = problem;
            this.placements = placements;
            MusicianScoreCache = musicianScoreCache;
            MusicianBlockedCache = musicianBlockedCache;
            MusicianDistanceScoreCache = musicianDistanceScoreCache;
            ScoreCache = scoreCache;

            this.occlusionFinder = new(this);

            foreach (var musician in problem.Musicians)
            {
                occlusionFinder.OnPlacementChanged(musician, Point.ORIGIN);
            }
        }

        public Point GetPlacement(Musician musician)
        {
            return Placements[musician.Index];
        }

        public void SetPlacement(Musician musician, Point loc)
        {
            var oldLoc = Placements[musician.Index];
            placements[musician.Index] = loc;
            occlusionFinder.OnPlacementChanged(musician, oldLoc);
        }

        public void Swap(int m0, int m1)
        {
            int instrument0 = Problem.Musicians[m0].Instrument;
            int instrument1 = Problem.Musicians[m1].Instrument;

            if (instrument0 == instrument1)
            {
                // Effectively a noop
                return;
            }

            var m0ScoreCache = MusicianScoreCache[m0];
            var m1ScoreCache = MusicianScoreCache[m1];

            for (int i = 0; i < Problem.Attendees.Count; i++)
            {
                ScoreCache -= m0ScoreCache[i];
                ScoreCache -= m1ScoreCache[i];
            }

            List<int> instrument0Musicians = new();
            List<int> instrument1Musicians = new();

            foreach (var musician in Problem.Musicians)
            {
                if (musician.Instrument == instrument0 && musician.Index != m0)
                {
                    instrument0Musicians.Add(musician.Index);
                }
                else if (musician.Instrument == instrument1 && musician.Index != m1)
                {
                    instrument1Musicians.Add(musician.Index);
                }
            }

            Point m0Placement = Placements[m0];
            double m0DistScoreCache = MusicianDistanceScoreCache[m0];
            foreach (var mi in instrument0Musicians)
            {
                double dist = m0Placement.Dist(Placements[mi]);
                m0DistScoreCache -= 1 / dist;
                MusicianDistanceScoreCache[mi] -= 1 / dist;
            }

            Point m1Placement = Placements[m1];
            double m1DistScoreCache = MusicianDistanceScoreCache[m1];
            foreach (var mi in instrument1Musicians)
            {
                double dist = m1Placement.Dist(Placements[mi]);
                m1DistScoreCache -= 1 / dist;
                MusicianDistanceScoreCache[mi] -= 1 / dist;
            }

            placements[m0] = m1Placement;
            placements[m1] = m0Placement;
            m0Placement = Placements[m0];
            m1Placement = Placements[m1];

            foreach (var mi in instrument0Musicians)
            {
                double dist = m0Placement.Dist(Placements[mi]);
                m0DistScoreCache += 1 / dist;
                MusicianDistanceScoreCache[mi] += 1 / dist;
            }

            foreach (var mi in instrument1Musicians)
            {
                double dist = m1Placement.Dist(Placements[mi]);
                m1DistScoreCache += 1 / dist;
                MusicianDistanceScoreCache[mi] += 1 / dist;
            }

            MusicianDistanceScoreCache[m0] = m0DistScoreCache;
            MusicianDistanceScoreCache[m1] = m1DistScoreCache;

            var m0BlockedCache = MusicianBlockedCache[placements[m0]];
            var m1BlockedCache = MusicianBlockedCache[placements[m1]];

            for (int i = 0; i < Problem.Attendees.Count; i++)
            {
                m0ScoreCache[i] = 0;
                m1ScoreCache[i] = 0;
                if (!m0BlockedCache.Contains(i))
                {
                    m0ScoreCache[i] = PairScore(m0, i);
                }

                if (!m1BlockedCache.Contains(i))
                {
                    m1ScoreCache[i] = PairScore(m1, i);
                }

                ScoreCache += m0ScoreCache[i];
                ScoreCache += m1ScoreCache[i];
            }
        }

        public Solution Copy()
        {
            Dictionary<int, List<long>> cacheCopy = new Dictionary<int, List<long>>();
            foreach (var kvp in MusicianScoreCache)
            {
                cacheCopy.Add(kvp.Key, new List<long>(kvp.Value));
            }

            Dictionary<int, double> distanceCacheCopy = new Dictionary<int, double>();
            foreach (var kvp in MusicianDistanceScoreCache)
            {
                distanceCacheCopy.Add(kvp.Key, kvp.Value);
            }

            return new Solution(Problem, new List<Point>(Placements), cacheCopy, distanceCacheCopy, MusicianBlockedCache, ScoreCache);
        }

        private void ResetCaches()
        {
            MusicianScoreCache = new Dictionary<int, List<long>>();
            MusicianDistanceScoreCache = new Dictionary<int, double>();
            MusicianBlockedCache = new Dictionary<Point, HashSet<int>>();
            for (int i = 0; i < Problem.Musicians.Count; i++)
            {
                MusicianScoreCache.Add(i, new List<long>());
                MusicianDistanceScoreCache.Add(i, 1);
                MusicianBlockedCache.Add(Placements[i], new HashSet<int>());
                for (int j = 0; j < Problem.Attendees.Count; j++)
                {
                    MusicianScoreCache[i].Add(0);
                }
            }

        }

        // If you want to actually use anything this thing caches, you'd better call this first.
        // Any changes (calls to SetLocation) will also require this to be re-called first.
        public long InitializeScore()
        {
            ScoreCache = 0;
            ResetCaches();
            for (int m0 = 0; m0 < Problem.Musicians.Count - 1; m0++)
            {
                for (int m1 = m0 + 1; m1 < Problem.Musicians.Count; m1++)
                {
                    Musician musician0 = Problem.Musicians[m0];
                    Musician musician1 = Problem.Musicians[m1];
                    if (musician0.Instrument != musician1.Instrument)
                    {
                        continue;
                    }

                    double distance = Placements[m0].Dist(Placements[m1]);
                    MusicianDistanceScoreCache[m0] += 1 / distance;
                    MusicianDistanceScoreCache[m1] += 1 / distance;
                }
            }

            for (int musicianIndex = 0; musicianIndex < Problem.Musicians.Count; musicianIndex++)
            {
                var musician = Problem.Musicians[musicianIndex];
                for (int attendeeIndex = 0; attendeeIndex < Problem.Attendees.Count; attendeeIndex++)
                {
                    var attendee = Problem.Attendees[attendeeIndex];

                    if (occlusionFinder.IsMusicianBlocked(musician, attendee))
                    {
                        MusicianBlockedCache[Placements[musicianIndex]].Add(attendeeIndex);
                        continue;
                    }

                    long tempScore = PairScore(musicianIndex, attendeeIndex);
                    MusicianScoreCache[musicianIndex][attendeeIndex] = tempScore;
                    ScoreCache += tempScore;
                }
            }

            return ScoreCache;
        }

        public long SupplyGradientToUI(int musicianIndex)
        {
            return Problem.Musicians[musicianIndex].Instrument;
            //return MusicianScoreCache[musicianIndex].Sum();
        }

        // Assumes no blocking!
        public long PairScore(int musicianIndex, int attendeeIndex)
        {
            return Problem.PairScore(Problem.Musicians[musicianIndex].Instrument, attendeeIndex, Placements[musicianIndex], MusicianDistanceScoreCache[musicianIndex]);
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

        public bool IsMusicianBlocked(Attendee attendee, Musician musician)
        {
            return IsMusicianBlocked(attendee.Index, musician.Index);
        }

        private bool IsMusicianBlocked(int attendeeIndex, int musicianIndex)
        {
            if (MusicianBlockedCache == null)
            {
                return false;
            }

            return MusicianBlockedCache[placements[musicianIndex]].Contains(attendeeIndex);
        }

        public bool IsMusicianBlocked(Point attendee, Musician musician, Musician blockingMusician)
        {
            return IsMusicianBlocked(attendee, musician, GetPlacement(blockingMusician), Musician.BLOCKING_RADIUS);
        }

        public bool IsMusicianBlocked(Point attendee, Musician musician, Pillar pillar)
        {
            return IsMusicianBlocked(attendee, musician, pillar.Center, pillar.Radius);
        }

        private bool IsMusicianBlocked(Point attendee, Musician musician, Point blockingLoc, double radius)
        {
            var musicianLoc = GetPlacement(musician);

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
            return dp.DotProduct(dp) < radius * radius;
        }

        public static Solution Read(string solutionPath, ProblemSpec problem)
        {
            var solutionJson = FileUtil.Read(solutionPath);
            var solution = ReadJson(solutionJson);
            solution.Problem = problem;
            solution.occlusionFinder = new(solution);

            return solution;
        }

        public static Solution ReadJson(string solutionJson)
        {
            var raw = JsonConvert.DeserializeObject<RawSolution>(solutionJson);
            return new Solution(raw.placements.ToList());
        }

        public string WriteJson()
        {
            return JsonConvert.SerializeObject(new RawSolution(placements.ToArray()));
        }

        private record class RawSolution(Point[] placements);
    }
}
