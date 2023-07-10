using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using UltimateQuadTree;
using QuadTrees;
using QuadTrees.QTreePointF;
using ICFP2023;


namespace ICFP2023
{
    public readonly struct MusicianLoc : IQuadTreeObjectBounds<MusicianLoc>, IPointFQuadStorable, IEqualityComparer<MusicianLoc>
    {
        public readonly int m;
        public readonly Point loc;

        public bool Equals(MusicianLoc obj, MusicianLoc other)
        {
            return obj.m == other.m && obj.loc.Equals(other.loc);
        }

        public int GetHashCode(MusicianLoc obj)
        {
            return HashCode.Combine(obj.m, obj.loc);
        }

        public MusicianLoc(int _m, Point _loc)
        {
            m = _m;
            loc = _loc;
            Point = new System.Drawing.PointF((float)loc.X, (float)loc.Y);
        }

        public double GetLeft(MusicianLoc mloc) => mloc.loc.X;
        public double GetRight(MusicianLoc mloc) => mloc.loc.X;
        public double GetTop(MusicianLoc mloc) => mloc.loc.Y;
        public double GetBottom(MusicianLoc mloc) => mloc.loc.Y;

        public System.Drawing.PointF Point { get; init; }
    }

    public class Solution
    {
        public ProblemSpec Problem { get; set; }
        public IReadOnlyList<Point> Placements => placements;
        private List<Point> placements;

        private Solution(List<Point> placements)
        {
            this.placements = placements;
        }

        public Dictionary<Point, Musician> WhosThere { get; init; }
        public long ScoreCache { get; private set; }

        // Index of musician to the list of scores for each attendee
        private Dictionary<int, List<long>> MusicianScoreCache;

        // Position of the musician to the list of attendees that are unblocked from that perspective
        private Dictionary<Point, HashSet<int>> MusicianUnblockedCache;

        // Musician to q(i) for playing together
        private Dictionary<int, double> MusicianDistanceScoreCache;

        private OcclusionFinder occlusionFinder;

        public long[] NScoreCache { get; init; }
        public long NScoreCacheTotal { get; set; }
        public double[,] NScoreDistCache { get; set; }
        public QuadTreePointF<MusicianLoc> MusicianTree { get; init; }

        public Solution(ProblemSpec problem) : this(problem, Point.ORIGIN)
        {}

        public Solution(ProblemSpec problem, Point initial)
        {
            this.Problem = problem;
            placements = new List<Point>();
            for (int i = 0; i < problem.Musicians.Count; i++)
            {
                placements.Add(initial);
            }

            MusicianTree = new(-1, -1, (int)Problem.RoomWidth, (int)Problem.RoomHeight);

            this.occlusionFinder = new(this);
            foreach (var musician in problem.Musicians)
            {
                occlusionFinder.OnPlacementChanged(initial, initial);
                MusicianTree.Add(new MusicianLoc(musician.Index, initial));
            }

            WhosThere = new Dictionary<Point, Musician>();
            NScoreCache = new long[problem.Musicians.Count];
            NScoreCacheTotal = 0;
            NScoreDistCache = new double[problem.Musicians.Count, problem.Musicians.Count];
        }

        private Solution(ProblemSpec problem,
            List<Point> placements,
            Dictionary<int, List<long>> musicianScoreCache,
            Dictionary<int, double> musicianDistanceScoreCache,
            Dictionary<Point, HashSet<int>> musicianUnblockedCache,
            long scoreCache,
            long[] nScoreCache,
            long nScoreCacheTotal,
            double[,] nScoreDistCache,
            QuadTreePointF<MusicianLoc> mtree)
        {
            Problem = problem;
            this.placements = placements;
            MusicianScoreCache = musicianScoreCache;
            MusicianUnblockedCache = musicianUnblockedCache;
            MusicianDistanceScoreCache = musicianDistanceScoreCache;
            ScoreCache = scoreCache;

            MusicianTree = mtree;

            this.occlusionFinder = new(this);
            foreach (var musician in problem.Musicians)
            {
                occlusionFinder.OnPlacementChanged(placements[musician.Index], Point.ORIGIN);
            }

            // WhosThere = new Dictionary<Point, Musician>();
            // foreach (var musician in problem.Musicians) {
            //     WhosThere.Add(placements[musician.Index], musician);
            // }
            NScoreCache = nScoreCache;
            NScoreCacheTotal = nScoreCacheTotal;
            NScoreDistCache = nScoreDistCache;
        }

        public Point GetPlacement(Musician musician)
        {
            return Placements[musician.Index];
        }

        public bool SetPlacement(Musician musician, Point loc, bool check=false)
        {
            var oldLoc = Placements[musician.Index];
            placements[musician.Index] = loc;
            occlusionFinder.OnPlacementChanged(loc, oldLoc);

            MusicianTree.Remove(new MusicianLoc(musician.Index, oldLoc));
            MusicianTree.Add(new MusicianLoc(musician.Index, loc));
            // if (check && WhosThere.ContainsKey(loc)) {
            //     return false;
            // }
            // WhosThere.Remove(oldLoc);
            // WhosThere.Add(loc, musician);
            return true;
        }

        public void SwapNoCache(Musician m0, Musician m1)
        {
            var loc0 = Placements[m0.Index];
            var loc1 = Placements[m1.Index];
            placements[m0.Index] = loc1;
            placements[m1.Index] = loc0;
            occlusionFinder.OnPlacementChanged(loc1, loc0);
            occlusionFinder.OnPlacementChanged(loc0, loc1);
            MusicianTree.Remove(new MusicianLoc(m0.Index, loc0));
            MusicianTree.Add(new MusicianLoc(m0.Index, loc1));
            MusicianTree.Remove(new MusicianLoc(m1.Index, loc1));
            MusicianTree.Add(new MusicianLoc(m1.Index, loc0));
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
                m0ScoreCache[i] = 0;
                m1ScoreCache[i] = 0;
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

            var changedMusicians = instrument0Musicians.Concat(instrument1Musicians);
            changedMusicians = changedMusicians.Append(m0);
            changedMusicians = changedMusicians.Append(m1);

            // Musicians with the same instrument now have different distance bonuses,
            // so we need to recompute their scores.
            foreach (int mi in changedMusicians)
            {
                var mScoreCache = MusicianScoreCache[mi];

                for (int i = 0; i < Problem.Attendees.Count; i++)
                {
                    ScoreCache -= mScoreCache[i];
                    mScoreCache[i] = 0;
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

            // Recompute the scores for the changed musicians now that the distance bonuses are known
            foreach (var mi in changedMusicians)
            {
                var mScoreCache = MusicianScoreCache[mi];

                foreach (int attendeeIndex in MusicianUnblockedCache[placements[mi]])
                {
                    long pairScore = PairScore(mi, attendeeIndex);
                    mScoreCache[attendeeIndex] = pairScore;
                    ScoreCache += pairScore;
                }
            }
        }

        public double GetScoreForMusician(int index)
        {
            if (MusicianScoreCache == null)
            {
                return 0;
            }

            return MusicianScoreCache[index].Sum();
        }

        public double GetScoreForAttendee(int index)
        {
            if (MusicianScoreCache == null)
            {
                return 0;
            }

            double score = 0;
            foreach (var kvp in MusicianScoreCache)
            {
                score += kvp.Value[index];
            }

            return score;
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

            QuadTreePointF<MusicianLoc>  MusicianTreeCopy = new(-1, -1, (int)Problem.RoomWidth, (int)Problem.RoomHeight);
            foreach (var n in MusicianTree.GetAllObjects()) {
                MusicianTreeCopy.Add(n);
            }

            return new Solution(
                Problem,
                new List<Point>(Placements),
                cacheCopy,
                distanceCacheCopy,
                MusicianUnblockedCache,
                ScoreCache,
                (long[])NScoreCache.Clone(),
                NScoreCacheTotal,
                (double[,])NScoreDistCache.Clone(),
                MusicianTreeCopy);
        }

        private void ResetCaches()
        {
            MusicianScoreCache = new Dictionary<int, List<long>>();
            MusicianDistanceScoreCache = new Dictionary<int, double>();
            MusicianUnblockedCache = new Dictionary<Point, HashSet<int>>();
            for (int i = 0; i < Problem.Musicians.Count; i++)
            {
                MusicianScoreCache.Add(i, new List<long>());
                MusicianDistanceScoreCache.Add(i, 1);
                HashSet<int> unblocked = new();
                MusicianUnblockedCache.Add(Placements[i], unblocked);

                for (int j = 0; j < Problem.Attendees.Count; j++)
                {
                    MusicianScoreCache[i].Add(0);
                    unblocked.Add(j);
                }
            }
        }

        // If you want to actually use anything this thing caches, you'd better call this first.
        // Any changes (calls to SetLocation) will also require this to be re-called first.
        public long InitializeScore(bool allscore=false)
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

            var instmuses = Problem.Musicians.OrderBy(x => x.Instrument).ToList();
            foreach (var musician in instmuses)
            {
                var musicianIndex = musician.Index;
                long mscore = 0;
                for (int attendeeIndex = 0; attendeeIndex < Problem.Attendees.Count; attendeeIndex++)
                {
                    var attendee = Problem.Attendees[attendeeIndex];

                    if (Scorer.IsBlocked(this, attendee, musician))
                    {
                        MusicianUnblockedCache[Placements[musicianIndex]].Remove(attendeeIndex);
                        continue;
                    }

                    long tempScore = PairScore(musicianIndex, attendeeIndex);
                    MusicianScoreCache[musicianIndex][attendeeIndex] = tempScore;
                    ScoreCache += tempScore;
                    mscore += tempScore;
                }
                if (allscore) Console.Error.WriteLine($"Score: {musician.Index,4:N0}\t{musician.Instrument,4:N0}\t{placements[musician.Index]} {mscore,16:N0}\t");
            }

            return ScoreCache;
        }

        public long SupplyGradientToUI(int musicianIndex)
        {
            //return Problem.Musicians[musicianIndex].Instrument;
            if (MusicianScoreCache == null)
            {
                return 0;
            }

            return 0;
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
                if (point == Point.INVALID) continue;

                double minX = Problem.StageBottomLeft.X + Musician.SOCIAL_DISTANCE;
                double maxX = Problem.StageBottomLeft.X + Problem.StageWidth - Musician.SOCIAL_DISTANCE;
                double minY = Problem.StageBottomLeft.Y + Musician.SOCIAL_DISTANCE;
                double maxY = Problem.StageBottomLeft.Y + Problem.StageHeight - Musician.SOCIAL_DISTANCE;

                if (point == Point.ORIGIN ||
                    (float)point.X < (float)minX || (float)point.X > (float)maxX ||
                    (float)point.Y < (float)minY || (float)point.Y > (float)maxY)
                {
                    Console.Error.WriteLine($"{point} not in bounds");
                    return false;
                }
            }

            // Check for musician social distancing
            bool valid = true;
            for (int i = 0; i < Placements.Count; i++)
            {
                if (Placements[i] == Point.INVALID) continue;
                for (int j = i + 1; j < Placements.Count; j++)
                {
                    if (Placements[j] == Point.INVALID) continue;
                    if (Placements[i].DistSq(Placements[j]) < Musician.SOCIAL_DISTANCE * Musician.SOCIAL_DISTANCE)
                    {
                        valid = false;
                        Console.Error.WriteLine($"{i} @ {Placements[i]} overlaps {j} @ {Placements[j]}");
                    }
                }
            }

            return valid;
        }

        public bool IsMusicianBlocked(Attendee attendee, Musician musician)
        {
            return IsMusicianBlocked(attendee.Index, musician.Index);
        }

        private bool IsMusicianBlocked(int attendeeIndex, int musicianIndex)
        {
            if (MusicianUnblockedCache == null)
            {
                return false;
            }

            return !MusicianUnblockedCache[placements[musicianIndex]].Contains(attendeeIndex);
        }

        public bool IsMusicianBlocked(Point attendee, Point musicianLoc, Point blockingMusicianLoc)
        {
            return IsMusicianBlocked(attendee, musicianLoc, blockingMusicianLoc, Musician.BLOCKING_RADIUS);
        }

        public bool IsMusicianBlocked(Point attendee, Musician musician, Pillar pillar)
        {
            return IsMusicianBlocked(attendee, GetPlacement(musician), pillar.Center, pillar.Radius);
        }

        private bool IsMusicianBlocked(Point attendee, Point musicianLoc, Point blockingLoc, double radius)
        {
            return Utils.IsLineOfSightBlocked(attendee, musicianLoc, blockingLoc, radius);
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
            var raw = JsonConvert.DeserializeObject<RawSolution>(solutionJson);
            return new Solution(raw.placements.ToList());
        }

        public string WriteJson()
        {
            // Have the scorer compute the score and output the volumes that maximized the score
            int[] volumes = new int[Problem.Musicians.Count];
            Scorer.ComputeScore(this, volumes);
            return JsonConvert.SerializeObject(new RawSolution(placements.ToArray(), volumes));
        }

        private record class RawSolution(Point[] placements, int[] volumes);

        public long[,,] PrecomputeStagePower()
        {
            long[,,] power = new long[Problem.InstrumentCount, (int)Problem.StageWidth, (int)Problem.StageHeight];
            // for (var x = 0; x < Problem.StageWidth; x++)
            Parallel.For(0, (int)Problem.StageWidth, x =>
            {
                // Console.WriteLine(x);
                for (var y = 0; y < Problem.StageHeight; y++)
                {
                    // Console.WriteLine("  " + y);
                    Point p = new Point(Problem.StageBottomLeft.X + x, Problem.StageBottomLeft.Y + y);
                    for (var i = 0; i < Problem.InstrumentCount; i++)
                    {
                        foreach (var a in Problem.Attendees)
                        {
                            power[i, (int)x, (int)y] += (long)(1000000 * a.Tastes[i] / (a.Location - p).MagnitudeSq);
                        }
                    }
                }
            // }
            });

            return power;
        }

        public static long[,,] ComputeGradients(long[,,] power)
        {
            int zLength = power.GetLength(0);
            int xLength = power.GetLength(1);
            int yLength = power.GetLength(2);

            // We'll have two values (dx, dy) for each point in 2D slice, hence the 3D array
            long[,,] gradients = new long[zLength, xLength, yLength];

            for (int z = 0; z < zLength; z++)
            {
                for (int x = 1; x < xLength - 1; x++)
                {
                    for (int y = 1; y < yLength - 1; y++)
                    {
                        long dx = (power[z, x + 1, y] - power[z, x - 1, y]) / 2;
                        long dy = (power[z, x, y + 1] - power[z, x, y - 1]) / 2;

                        // Store the magnitude of gradient vector at each point
                        gradients[z, x, y] = (long)Math.Sqrt(dx * dx + dy * dy);
                    }
                }
            }
            return gradients;
        }

        public int MusicianOverlaps(int m, Point loc) {
            return MusicianOverlapsB(m, loc);
        }

        public int MusicianOverlapsC(int m, Point loc) {
            var a = MusicianOverlapsA(m, loc);
            var b = MusicianOverlapsB(m, loc);
            if (a != -1 && b != -1 || a == -1 && b == -1) {
                return a;
            }
            // Something went bad
            Console.WriteLine($"Overlap failed {a} {b}");
            a = MusicianOverlapsA(m, loc);
            b = MusicianOverlapsB(m, loc);
            return a;
        }

        public int MusicianOverlapsB(int m, Point loc)
        {
            var neighbors = new List<MusicianLoc>();
            MusicianTree.GetObjects(
                new System.Drawing.RectangleF((float)loc.X-10, (float)loc.Y-10, 20, 20), neighbors);
            foreach (var n in neighbors) {
                if (n.m == m) continue;
                if ((loc - n.loc).Magnitude < Musician.SOCIAL_DISTANCE) {
                    return n.m;
                }
            }

            return -1;
        }

        public int MusicianOverlapsA(int m, Point loc)
        {
            foreach (var mp in Problem.Musicians)
            {
                if (mp.Index == m) continue;

                var dist = (loc - Placements[mp.Index]).Magnitude;
                if (dist<Musician.SOCIAL_DISTANCE)
                {
                    return mp.Index;
                }
            }
            return -1;
        }

        public long NScoreMusicianOverlap(Musician m, long score)
        {
            foreach (var mp in Problem.Musicians)
            {
                if (mp.Index == m.Index) continue;

                NScoreDistCache[m.Index, mp.Index] = (Placements[m.Index] - Placements[mp.Index]).Magnitude;
                NScoreDistCache[mp.Index, m.Index] = NScoreDistCache[m.Index, mp.Index];

                if (NScoreDistCache[m.Index, mp.Index] < Musician.SOCIAL_DISTANCE)
                {
                    // Huge penalty if too close.
                    score = (long)(score * 0.10 * NScoreDistCache[m.Index, mp.Index] / Musician.SOCIAL_DISTANCE);
                    // score = -(long)(score);
                    break;
                }
            }

            return score;
        }

        public long NScoreMusician(Musician m, int n=5000, bool occlusion=true)
        {
            n = Math.Min(n, Problem.Musicians.Count);
            long score = 0;

            for (int i = 0; i < n; i++) {
                var attendeeIndex = Problem.Strongest[m.Instrument, i];
                var attendee = Problem.Attendees[attendeeIndex];
                var pairScore = PairScore(m.Index, attendeeIndex);
                if (!occlusion || !occlusionFinder.IsMusicianBlocked(m, attendee)) {
                    score += pairScore;
                } else {
                    // Allow some signal through to help signal the annealer.
                    score += (long)(pairScore * 0.10);
                }
            }

            score = NScoreMusicianOverlap(m, score);

            return score;
        }

        public long NScoreFull(int n=5000, bool occlusion=true)
        {
            n = Math.Min(n, Problem.Musicians.Count);
            Problem.LoadMetaDataStrongest();

            NScoreCacheTotal = 0;
            foreach (var m in Problem.Musicians) {
                NScoreCache[m.Index] = NScoreMusician(m, n, occlusion);
                NScoreCacheTotal += NScoreCache[m.Index];
            }

            return NScoreCacheTotal;
        }

        public long NScoreWithCache(int updateMusician = -1, int n=5000, bool occlusion=true)
        {
            n = Math.Min(n, Problem.Musicians.Count);
            if (updateMusician < 0) return NScoreCacheTotal;

            Musician musician = Problem.Musicians[updateMusician];
            NScoreCacheTotal -= NScoreCache[updateMusician];
            NScoreCache[updateMusician] = NScoreMusician(musician, n, occlusion);
            NScoreCacheTotal += NScoreCache[updateMusician];

            return NScoreCacheTotal;
        }

        public static long RUNID = DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 10000000 / 10;

        public void Render(bool all=false)
        {
            var p = Problem;

            var height = all ? p.RoomHeight : p.StageHeight;
            var width = all ? p.RoomWidth : p.StageWidth;

            using var image = new Image<Rgba32>((int)width, (int)height);

            if (all) {
                var pen = Pens.Solid(Color.Black, 1); // Red color and thickness of 5
                image.Mutate(ctx => ctx.Draw(pen, new RectangleF((float)p.StageLeft, (float)p.StageBottom, (float)p.StageWidth, (float)p.StageHeight)));
                image.Mutate(ctx => ctx.Draw(pen, new RectangleF((float)p.StageFenceLeft, (float)p.StageFenceBottom, (float)p.StageWidth - 20, (float)p.StageHeight - 20)));
            }

            foreach (var m in p.Musicians)
            {
                var pl = Placements[m.Index];
                var dotCenter = all
                    ? new PointF((int)(pl.X), (int)(pl.Y)) // The position of the dot:
                    : new PointF((int)(pl.X - p.StageLeft), (int)(pl.Y - p.StageBottom)); // The position of the dot
                float dotRadius = 5; // The radius of the dot

                var color = new Hsv(new Vector3(360 * m.Instrument / p.InstrumentCount, 1, 1));
                var c = ColorSpaceConverter.ToRgb(color);
                var rgb = Color.FromRgb((byte)(c.R * 255), (byte)(c.G * 255), (byte)(c.B * 255));

                image.Mutate(x => x.Fill(
                    rgb,
                    new EllipsePolygon(dotCenter, dotRadius) // The dot
                ));
            }

            if (all) {
                foreach (var m in p.Attendees)
                {
                    var pl = m.Location;
                    var dotCenter = new PointF((float)pl.X, (float)pl.Y); // The position of the dot
                    float dotRadius = 2; // The radius of the dot

                    image.Mutate(x => x.Fill(
                        Color.Black, // The color of the dot
                        new EllipsePolygon(dotCenter, dotRadius) // The dot
                    ));
                }
            }

            try {
                image.Save($"render/{RUNID}-{Problem.ProblemNumber}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 1000000}" + (all ? "-all" : "") + ".png");
            } catch (Exception e) {}
        }

    }

}
