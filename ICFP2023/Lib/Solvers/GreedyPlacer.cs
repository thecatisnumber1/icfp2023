using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class GreedyPlacer
    {
        public static void Solve(ProblemSpec problem, SharedSettings settings, UIAdapter ui)
        {
            Solution solution = new Solution(problem);
            Place(solution, ui);
            ui.Render(solution);
            AnnealingSolver.Solve(solution, AnnealingSolver.ComputeCost, 30000, 1000000);
        }

        public static void Place(Solution solution, UIAdapter ui)
        {
            HashSet<Point> gridPoints = new HashSet<Point>(Utils.GridPoints(solution.Problem));
            List<Musician> sortedMusicians =
                solution.Problem.Musicians.OrderByDescending(m => ScoreMusician(solution.Problem, m, gridPoints.First())).ToList();

            foreach (Musician m in sortedMusicians)
            {
                long bestScore = long.MinValue;
                Point bestPoint = Point.ORIGIN;
                foreach (Point p in gridPoints)
                {
                    long score = ScoreMusician(solution.Problem, m, p);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPoint = p;
                    }
                }

                solution.SetPlacement(m, bestPoint);
                ui.Render(solution);
                gridPoints.Remove(bestPoint);
            }
        }

        // Assumes no blocking!!
        private static long ScoreMusician(ProblemSpec problem, Musician musician, Point location)
        {
            long score = 0;
            for (int i = 0; i < problem.Attendees.Count; i++)
            {
                score += problem.PairScore(musician.Index, i, location);
            }

            return score;
        }
    }
}
