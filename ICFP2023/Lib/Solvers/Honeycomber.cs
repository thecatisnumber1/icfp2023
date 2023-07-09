using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class Honeycomber
    {
        public static Solution FixedPointAnnealSolve(ProblemSpec problem, SharedSettings settings, UIAdapter ui)
        {
            List<Point> fixedPoints = new List<Point>();
            if (fixedPoints.Count < problem.Musicians.Count)
            {
                Console.WriteLine("Not enough fixed points!");
                return new Solution(problem);
            }

            List<int> matches = FixedPointMatcher.FindMatching(problem, fixedPoints, ui, 90000, 200000);
            Solution solution = FixedPointSolution.MatchingToSolution(problem, fixedPoints, matches);

            Console.WriteLine($"{solution.InitializeScore()}");
            ui.Render(solution);
            ColorUtil.ColorInstruments(ui, problem);

            return solution;
        }
    }
}
