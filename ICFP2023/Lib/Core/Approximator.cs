using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class Approximator
    {
        private const int CELL_SIZE = 40;

        public static ProblemSpec Approximate(ProblemSpec problem)
        {
            ProblemSpec approx = new(
                problem.RoomWidth,
                problem.RoomHeight,
                problem.StageWidth,
                problem.StageHeight,
                problem.StageBottomLeft,
                problem.Musicians,
                ApproxAttendees(problem),
                FilterPillars(problem)
            );

            if (problem.ProblemName != null)
            {
                approx.SetProblemName(problem.ProblemName);
            }

            return approx;
        }

        public static Solution ToRealSolution(ProblemSpec originalProblem, Solution approximateSolution)
        {
            Solution real = new(originalProblem);
            
            foreach (var musician in originalProblem.Musicians)
            {
                real.SetPlacement(musician, approximateSolution.GetPlacement(musician));
            }

            return real;
        }

        private static List<Attendee> ApproxAttendees(ProblemSpec problem)
        {
            var attendees = problem.Attendees;
            int nTastes = attendees[0].Tastes.Count;
            double stageHeight = problem.StageHeight;
            double stageWidth = problem.StageWidth;
            double perimeter = stageHeight * 2 + stageWidth * 2;
            double roomSize = Math.Sqrt(problem.RoomHeight * problem.RoomHeight + problem.RoomWidth * problem.RoomWidth);

            int maxDistBuckets = PolarToBucket((roomSize, 1)).Item1 + 1;
            int maxPerimBuckets = (int)Math.Ceiling(perimeter / CELL_SIZE);

            var buckets = new List<Attendee>[maxDistBuckets, maxPerimBuckets];

            foreach (var attendee in attendees)
            {
                if(attendee.Index == 189)
                {

                }

                var polar = StagePolar(problem.Stage, attendee.Location);
                var (bx, by) = PolarToBucket(polar);

                if (buckets[bx, by] == null)
                {
                    buckets[bx, by] = new();
                }

                buckets[bx, by].Add(attendee);
            }

            List<Attendee> approxAttendees = new();

            // Do not approximate attendees in distance bucket 0.
            // Their precise location is potentially very important.
            for (int j = 0; j < maxPerimBuckets; j++)
            {
                if (buckets[0, j] == null)
                {
                    continue;
                }

                foreach (var attendee in buckets[0, j])
                {
                    approxAttendees.Add(new(approxAttendees.Count, attendee.Location, attendee.Tastes));
                }
            }

            for (int i = 1; i < maxDistBuckets; i++)
            {
                for (int j = 0; j < maxPerimBuckets; j++)
                {
                    if (buckets[i, j] == null)
                    {
                        continue;
                    }

                    double sumX = 0;
                    double sumY = 0;
                    double[] tastes = new double[nTastes];

                    foreach (var attendee in buckets[i, j])
                    {
                        sumX += attendee.Location.X;
                        sumY += attendee.Location.Y;

                        // For attendees that are in roughly the same area, their distance to
                        // musicians will be about the same so we can just sum the tastes.
                        // The error is inversely proportional to the distance, but since the
                        // buckets are smaller closer to the stage there is less distance error.
                        for (int k = 0; k < attendee.Tastes.Count; k++)
                        {
                            tastes[k] += attendee.Tastes[k];
                        }
                    }

                    // The location of the approximate attendee is the centroid of all in the bucket
                    Point loc = new(sumX / buckets[i, j].Count, sumY / buckets[i, j].Count);

                    approxAttendees.Add(new(approxAttendees.Count, loc, tastes.ToList()));
                }
            }

            return approxAttendees;
        }

        private static (int, int) PolarToBucket((double, double) polar)
        {
            var (dist, perim) = polar;

            // Bucketize by distance such that the buckets get larger farther away.
            // Use sqrt because that is how the sound contribution falls off.
            int distBucket = (int)Math.Sqrt(dist / CELL_SIZE);

            // How many cells away from the stage will have this same bucket
            int bucketSize = ((distBucket + 1) * (distBucket + 1)) - (distBucket * distBucket);

            int perimBucket = (int)(perim / (CELL_SIZE * bucketSize));

            return (distBucket, perimBucket);
        }

        private static (double, double) StagePolar(Rect stage, Point loc)
        {
            var (ax, ay) = loc;
            double startAngle = 0;

            // Directly to the left of the stage
            if (ax <= stage.Left && ay <= stage.Top && ay >= stage.Bottom)
            {
                return (stage.Left - ax, ay - stage.Bottom);
            }

            startAngle += stage.Top - stage.Bottom;

            // To the top-left corner of the stage
            if (ax <= stage.Left && ay > stage.Top)
            {
                return (loc.Dist(stage.TopLeft), startAngle);
            }

            // Directly to the top of the stage
            if (ay >= stage.Top && ax <= stage.Right && ax >= stage.Left)
            {
                return (ay - stage.Top, startAngle + ax - stage.Left);
            }

            startAngle += stage.Right - stage.Left;

            // To the top-right corner of the stage
            if (ay >= stage.Top && ax > stage.Right)
            {
                return (loc.Dist(stage.TopRight), startAngle);
            }

            // Directly to the right of the stage
            if (ax >= stage.Right && ay <= stage.Top && ay >= stage.Bottom)
            {
                return (ax - stage.Right, startAngle + stage.Top - ay);
            }

            startAngle += stage.Top - stage.Bottom;

            // To the bottom-right corner of the stage
            if (ax >= stage.Right && ay < stage.Bottom)
            {
                return (loc.Dist(stage.BottomRight), startAngle);
            }

            // Directly to the bottom of the stage
            if (ay <= stage.Bottom && ax <= stage.Right && ax >= stage.Left)
            {
                return (stage.Bottom - ay, startAngle + stage.Right - ax);
            }

            startAngle += stage.Right - stage.Left;

            // To the bottom-left corner of the stage
            return (loc.Dist(stage.BottomLeft), startAngle);
        }

        // Remove pillars that are far from the stage
        private static List<Pillar> FilterPillars(ProblemSpec problem)
        {
            List<Pillar> filtered = new();

            foreach (var pillar in problem.Pillars)
            {
                var (stageDist, _) = StagePolar(problem.Stage, pillar.Center);

                // Any pillars within the first cell of the stage should be kept
                // because attendees in those cells will remain unapproximated.
                if (stageDist - pillar.Radius <= CELL_SIZE)
                {
                    filtered.Add(new(filtered.Count, pillar.Center, pillar.Radius));
                }
            }

            return filtered;
        }
    }
}
