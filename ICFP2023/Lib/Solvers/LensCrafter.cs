using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class LensCrafter
    {
        public const double MUSICIAN_SPACING = 10 + 1E-8;

        public static List<Musician> AddLens(Solution solution, List<Point> fixedPoints)
        {
            Console.WriteLine("LensCrafter started.  Initializing score.");
            solution.InitializeScore();

            // Find the highest scoring attendee
            Attendee focus = GetHighestScoringAttendee(solution);
            Console.WriteLine(
                $"Found best attendee with {(solution.GetScoreForAttendee(focus.Index) / solution.ScoreCache).ToString("P2")} of total score.");

            // Find the group of musicians that contribute to their score (including the playing together folks).
            Musician starPlayer;
            List<Point> backupPlayers;
            GetStarAndBackupPlayers(solution, focus, out starPlayer, out backupPlayers);

            // Compute a lense facing them
            Side side = FindExitSide(solution, focus, starPlayer);
            Lens lens = CraftLens(focus.Location, side, Math.Min(backupPlayers.Count / 2, 6));

            // Collide out the musicians who would be in the way (hopefully these are the people who would be part of the lense!)
            List<Musician> lensParticipants, lookingForNewJob;
            AssignJobsToCollidedMusicians(solution, starPlayer, lens, fixedPoints, out lensParticipants, out lookingForNewJob);

            fixedPoints.AddRange(lens.Points);

            // Need to do fixedPointsSolver on the remaining musicians
            return lookingForNewJob;
        }

        private static void AssignJobsToCollidedMusicians(
            Solution solution, Musician starPlayer, 
            Lens lens,
            List<Point> fixedPoints,
            out List<Musician> lensParticipants, 
            out List<Musician> lookingForNewJob)
        {
            HashSet<int> booted = new HashSet<int>();
            List<Point> fixedPointsToRemove = new List<Point>();
            foreach (Point p in lens.Points)
            {
                foreach (Musician m in solution.Problem.Musicians)
                {
                    if (Utils.MusiciansCollide(p, solution.Placements[m.Index]))
                    {
                        booted.Add(m.Index);
                    }
                }

                foreach (Point fp in fixedPoints)
                {
                    if (Utils.MusiciansCollide(p, fp))
                    {
                        fixedPointsToRemove.Add(fp);
                    }
                }
            }

            foreach (Point fp in fixedPointsToRemove)
            {
                fixedPoints.Remove(fp);
            }

            List<Point> newFixedPoints = fixedPoints.Where(x => x.Dist(lens.Focus) > lens.Radius).ToList();
            fixedPoints.Clear();
            fixedPoints.AddRange(newFixedPoints);

            lensParticipants = new List<Musician>();
            lookingForNewJob = new List<Musician>();
            foreach (int i in booted)
            {
                if (solution.Problem.Musicians[i].Instrument == starPlayer.Instrument && lensParticipants.Count < lens.Points.Count)
                {
                    lensParticipants.Add(solution.Problem.Musicians[i]);
                }
                else
                {
                    lookingForNewJob.Add(solution.Problem.Musicians[i]);
                }
            }
        }

        private static Side FindExitSide(Solution solution, Attendee focus, Musician starPlayer)
        {

            // Find the side of the stage they are on
            Side side = null;
            foreach (Side s in solution.Problem.Stage.Sides)
            {
                if (s.LineSegementIntersects(focus.Location, solution.Placements[starPlayer.Index]))
                {
                    side = s;
                    break;
                }
            }

            return side;
        }

        private static void GetStarAndBackupPlayers(Solution solution, Attendee focus, out Musician starPlayer, out List<Point> backupPlayers)
        {
            starPlayer = solution.GetBestPlayerForAttendee(focus);
            backupPlayers = new List<Point>();
            foreach (Musician m in solution.Problem.Musicians)
            {
                if (m.Instrument == starPlayer.Instrument && solution.Placements[m.Index].Dist(solution.Placements[starPlayer.Index]) < 100)
                {
                    backupPlayers.Add(solution.Placements[m.Index]);
                }
            }
        }

        private static Attendee GetHighestScoringAttendee(Solution solution)
        {
            Attendee best = solution.Problem.Attendees.OrderByDescending(x => solution.GetScoreForAttendee(x.Index)).ToList()[0];
            return best;
        }

        public record Lens(List<Point> Points, Point Focus, double Radius);

        public static Lens CraftLens(Point focus, Side side, int numWatchers)
        {
            double lowerRadius = numWatchers * 10 / Math.PI;
            double upperRadius = lowerRadius * 20;
            while (upperRadius - lowerRadius > 1E-3)
            {
                double radius = (lowerRadius + upperRadius) / 2;
                Lens lens = CraftLens(focus, side, radius, numWatchers);
                if (lens == null)
                {
                    lowerRadius = radius;
                    continue;
                }

                double distanceFromSide = -side.OutwardComponent(lens.Points[0]);
                if (distanceFromSide > 10)
                {
                    upperRadius = radius;
                }
                else
                {
                    lowerRadius = radius;
                }
            }

            return CraftLens(focus, side, lowerRadius, numWatchers);
        }

        public static Lens CraftLens(Point focus, Side side, double radius, int numWatchers)
        {
            if (numWatchers < 1)
            {
                throw new Exception("That's not a lens.");
            }

            List<Point> results = new List<Point>();
            double seperationAngle = CircleSeparationAngle(radius);
            double totalAngle = seperationAngle * numWatchers;
            if (totalAngle > Math.PI * 2)
            {
                return null;
            }

            List<Point> watchers = new List<Point>();
            for (int i = 0; i <= numWatchers; i++)
            {
                double angle = totalAngle / 2 - i * seperationAngle;
                double dAlong = radius * Math.Sin(angle);
                double dOutward = -radius * Math.Cos(angle);
                Point current = focus + dOutward * side.Outward + dAlong * side.Along;
                results.Add(current);

                if (i != 0)
                {
                    watchers.Add(FitWatcher(results[i - 1], results[i]));
                }
            }

            results.AddRange(watchers);
            return new Lens(results, focus, radius);
        }

        private static double CircleSeparationAngle(double radius)
        {
            double cos = 1 - (MUSICIAN_SPACING * MUSICIAN_SPACING) / (2 * radius * radius);
            return Math.Acos(cos);
        }

        private static Point FitWatcher(Point p0, Point p1)
        {
            Vec along = p1 - p0;
            Vec outward = along.RotateCCW();
            return p0 + along / 2 + outward * Math.Sqrt(3) / 2;
        }
    }
}
