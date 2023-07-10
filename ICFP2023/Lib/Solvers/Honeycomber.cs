using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class Honeycomber
    {
        public static Solution Solve(ProblemSpec problem, SharedSettings settings, UIAdapter ui)
        {
            List<Point> fixedPoints = GetEdgePoints(8, problem.Stage);
            if (fixedPoints.Count < problem.Musicians.Count)
            {
                Console.WriteLine("Not enough fixed points!");
                return new Solution(problem);
            }

            List<int> matches = FixedPointMatcher.FindMatching(problem, fixedPoints, ui, 20000, 200000);
            Solution solution = FixedPointSolution.MatchingToSolution(problem, fixedPoints, matches);

            Console.WriteLine($"{solution.InitializeScore()}");
            ui.Render(solution);
            ColorUtil.ColorInstruments(ui, problem);

            return solution;
        }

        public static List<Point> GetEdgePoints(int rows, Rect stage)
        {
            List<Point> edgePoints = new List<Point>();
            List<Side> sides = stage.Shrink(10).Sides;
            for (int layer = 0; layer < rows; layer++)
            {
                for (int sideIndex = 0; sideIndex < sides.Count; sideIndex++)
                {
                    Side currentSide = sides[sideIndex];
                    AddRowPoints(currentSide, edgePoints);

                    sides[sideIndex] = currentSide.Shrink(5).Translate(currentSide.Outward * -(Math.Sqrt(3) * 5 + 1E-12));
                }
            }

            return edgePoints;
        }

        public static void AddRowPoints(Side side, List<Point> points)
        {
            int count = ((int)side.Length) / 10;
            Point currentPoint = side.Left;
            Vec step = side.Along * 10;
            for (int i = 0; i < count; i++)
            {
                if (!Utils.MusiciansCollide(points, currentPoint))
                {
                    points.Add(currentPoint);
                }

                currentPoint += step;
            }
        }
    }
}
