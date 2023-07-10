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

        public static List<Point> CraftLens(Point focus, Side side, int numWatchers)
        {
            double lowerRadius = numWatchers * 10 / Math.PI;
            double upperRadius = lowerRadius * 20;
            while (upperRadius - lowerRadius > 1E-3)
            {
                double radius = (lowerRadius + upperRadius) / 2;
                List<Point> lens = CraftLens(focus, side, radius, numWatchers);
                if (lens == null)
                {
                    lowerRadius = radius;
                    continue;
                }

                double distanceFromSide = -side.OutwardComponent(lens[0]);
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

        public static List<Point> CraftLens(Point focus, Side side, double radius, int numWatchers)
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
            return results;
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
