using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class Utils
    {
        public static readonly double GRID_SIZE = 10.0;

        public delegate double Score(double wastedSpace, Side stageSide, Attendee crack);
        record CrackPlacements(List<Attendee> attendees, double score);

        public static List<Attendee> SelectAttendees(ProblemSpec problem, Side side, Score score)
        {
            // Filter attendees (blocked, etc.)
            List<Attendee> candidates = FilterAttendees(problem, side);

            // Sort attendees by length from start of path (along side)
            candidates = candidates.OrderBy(x => side.AlongComponent(x.Location)).ToList();

            // foreach prefix of the sorted attendees list
            List<CrackPlacements> optimalSubsets = new();
            Side musicianSide = side.Shrink(GRID_SIZE / 2);
            foreach (Attendee a in candidates)
            {
                CrackPlacements bestPlacements =
                    new(new List<Attendee>() { a }, ScoreSpan(musicianSide, a, score, musicianSide.Left, a.Location));
                foreach (CrackPlacements subset in optimalSubsets)
                {
                    if (musicianSide.Along.DotProduct(a.Location - subset.attendees[^1].Location) < GRID_SIZE)
                    {
                        continue;
                    }

                    double innerScore = subset.score + ScoreSpan(musicianSide, a, score, subset.attendees[^1].Location, a.Location);
                    if (innerScore > bestPlacements.score)
                    {
                        var newAttendees = subset.attendees.ToList();
                        newAttendees.Add(a);
                        bestPlacements = new CrackPlacements(newAttendees, innerScore);
                    }
                }

                optimalSubsets.Add(bestPlacements);
            }

            CrackPlacements best =
                new(new List<Attendee>(), ScoreSpan(musicianSide, null, score, musicianSide.Left, musicianSide.Right));
            foreach (CrackPlacements crackPlacements in optimalSubsets)
            {
                double innerScore = crackPlacements.score + ScoreSpan(musicianSide, null, score, crackPlacements.attendees[^1].Location, musicianSide.Right);
                if (innerScore > best.score)
                {
                    best = new CrackPlacements(crackPlacements.attendees, innerScore);
                }
            }

            return best.attendees;
        }

        public static bool MusiciansCollide(Point m0, Point m1)
        {
            return m0.DistSq(m1) < GRID_SIZE * GRID_SIZE;
        }

        public static bool MusiciansCollide(List<Point> points, Point m)
        {
            foreach (Point p in points)
            {
                if (MusiciansCollide(p, m))
                {
                    return true;
                }
            }

            return false;
        }

        private static double ScoreSpan(Side musicianSide, Attendee a, Score score, Point spanStart, Point spanEnd)
        {
            double spanLength = musicianSide.Along.DotProduct(spanEnd - spanStart);
            double wastedSpace = spanLength % GRID_SIZE;
            return score(wastedSpace, musicianSide, a);
        }


        private static List<Attendee> FilterAttendees(ProblemSpec problem, Side side)
        {
            side = side.Shrink(GRID_SIZE * 1.5);
            List<Attendee> candidates = new List<Attendee>();
            foreach (Attendee attendee in problem.Attendees)
            {
                // Are they on the right side?
                double alongCoord = side.AlongComponent(attendee.Location);
                if (alongCoord < 0 || alongCoord > side.Length)
                {
                    continue;
                }

                double outwardCoord = side.OutwardComponent(attendee.Location);
                if (outwardCoord < 0)
                {
                    continue;
                }

                Point projected = side.Left + alongCoord * side.Along;
                bool blocked = false;
                foreach (Pillar pillar in problem.Pillars)
                {
                    if (IsLineOfSightBlocked(attendee.Location, projected, pillar.Center, pillar.Radius))
                    {
                        blocked = true;
                        break;
                    }
                }

                if (!blocked)
                {
                    candidates.Add(attendee);
                }
            }

            return candidates;
        }

        public static bool IsLineOfSightBlocked(Point attendee, Point source, Point blockingLoc, double radius)
        {
            var musicianLoc = source;

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

        public static List<Point> GridPoints(Rect rect)
        {
            List<Point> gridPoints = new List<Point>();
            for (double x = rect.Left + GRID_SIZE; x < rect.Right - GRID_SIZE; x += GRID_SIZE)
            {
                for (double y = rect.Bottom + GRID_SIZE; y < rect.Top - GRID_SIZE; y += GRID_SIZE)
                {
                    gridPoints.Add(new Point(x, y));
                }
            }

            return gridPoints;
        }

        // Starting corner is assumed to be on the corner of the stage.  Use StageBottomLeft, StageBottomRight, etc.
        public static List<Point> EdgePoints(ProblemSpec problem, Point stageStartingCorner)
        {
            List<Point> edgePoints = new List<Point>();
            Vec vertical;
            double oppositeX;
            double oppositeY;
            if (stageStartingCorner.Y == problem.StageBottom)
            {
                vertical = GRID_SIZE * Vec.NORTH;
                oppositeY = problem.StageTop;
            }
            else
            {
                vertical = GRID_SIZE * Vec.SOUTH;
                oppositeY = problem.StageBottom;
            }

            Vec horizontal;
            if (stageStartingCorner.X == problem.StageLeft)
            {
                horizontal = GRID_SIZE * Vec.EAST;
                oppositeX = problem.StageRight;
            }
            else
            {
                horizontal = GRID_SIZE * Vec.WEST;
                oppositeX = problem.StageLeft;
            }

            Point startingCorner = stageStartingCorner + horizontal + vertical;
            edgePoints.Add(startingCorner);
            AddPointsAlongEdge(edgePoints, problem, startingCorner, horizontal);
            Point horizontalLast = edgePoints.Last();
            AddPointsAlongEdge(edgePoints, problem, startingCorner, vertical);
            Point verticalLast = edgePoints.Last();

            Point oppositeCorner = new Point(oppositeX, oppositeY) - horizontal - vertical;
            edgePoints.Add(oppositeCorner);
            AddPointsAlongEdge(edgePoints, problem, oppositeCorner, -horizontal);
            if (verticalLast.DistSq(edgePoints.Last()) <= 100.0000001)
            {
                edgePoints.RemoveAt(edgePoints.Count - 1);
            }

            AddPointsAlongEdge(edgePoints, problem, oppositeCorner, -vertical);
            if (horizontalLast.DistSq(edgePoints.Last()) <= 100.0000001)
            {
                edgePoints.RemoveAt(edgePoints.Count - 1);
            }

            return edgePoints;
        }

        // startingCorner already accounts for the margin
        private static void AddPointsAlongEdge(List<Point> edgePoints, ProblemSpec problem, Point startingCorner, Vec step)
        {
            for (Point p = startingCorner + step; IsPointOnStage(p, problem); p += step)
            {
                edgePoints.Add(p);
            }
        }

        private static bool IsPointOnStage(Point p, ProblemSpec problem)
        {
            return p.X >= problem.StageLeft + GRID_SIZE && 
                p.X <= problem.StageRight - GRID_SIZE && 
                p.Y >= problem.StageBottom + GRID_SIZE && 
                p.Y <= problem.StageTop - GRID_SIZE;
        }
    }
}
