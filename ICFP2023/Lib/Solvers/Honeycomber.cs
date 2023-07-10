using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class Honeycomber
    {
        private const int ANNEAL_MS = 90000;

        public static Solution Solve(ProblemSpec problem, SharedSettings settings, UIAdapter ui)
        {
            List<Point> fixedPoints = GetEdgePoints(8, problem.Stage);
            if (fixedPoints.Count < problem.Musicians.Count)
            {
                Console.WriteLine("Not enough fixed points!");
                return new Solution(problem);
            }

            List<int> matches = FixedPointMatcher.FindMatching(problem, fixedPoints, ui, ANNEAL_MS, 200000);
            Solution solution = FixedPointSolution.MatchingToSolution(problem, fixedPoints, matches);

            Console.WriteLine($"{solution.InitializeScore()}");
            ui.Render(solution);
            //ColorUtil.ColorInstruments(ui, problem);

            /*LensCrafter.AddLens(solution, fixedPoints);
            matches = FixedPointMatcher.FindMatching(problem, fixedPoints, ui, ANNEAL_MS, 200000);
            solution = FixedPointSolution.MatchingToSolution(problem, fixedPoints, matches);
            ui.Render(solution);
            Console.WriteLine($"{solution.InitializeScore()}");*/

            return solution;
        }

        public static List<Point> GetEdgePoints(int rows, Rect stage)
        {
            List<Point> edgePoints = new List<Point>();
            List<Side> sides = stage.Shrink(10).Sides;
            List<(double spacing, double depth)> sideSpacing = new List<(double spacing, double depth)>();
            foreach (Side side in sides)
            {
                int count = ((int)side.Length) / 10;
                double spacing = 10 + (side.Length - count * 10) / (count - 1);
                double halfSpacing = spacing / 2;
                double depth = Math.Sqrt(100 - halfSpacing * halfSpacing) + 1E-12;
                sideSpacing.Add((spacing, depth));
            }

            for (int layer = 0; layer < rows; layer++)
            {
                for (int sideIndex = 0; sideIndex < sides.Count; sideIndex++)
                {
                    Side currentSide = sides[sideIndex];
                    AddRowPoints(currentSide, edgePoints, sideSpacing[sideIndex].spacing);
                    sides[sideIndex] = currentSide.Shrink(sideSpacing[sideIndex].spacing / 2).Translate(currentSide.Outward * -(sideSpacing[sideIndex].depth));
                }
            }

            return edgePoints;
        }

        public static void AddRowPoints(Side side, List<Point> points, double spacing)
        {
            int count = ((int)side.Length) / 10;
            Point currentPoint = side.Left;
            Vec step = side.Along * spacing;
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
