using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class GreedyPlacer
    {
        public static Solution Solve(ProblemSpec problem, SharedSettings settings, UIAdapter ui)
        {
            Solution solution = new Solution(problem);
            Place(solution, ui);
            ui.Render(solution);
            return AnnealingSolver.Solve(solution, AnnealingSolver.ComputeCost, 45000, 1000000);
        }

        // Deal with vuvuzelas later
        // Also with picking better corners
        public static void Place(Solution solution, UIAdapter ui)
        {
            HashSet<Point> edgePoints = new HashSet<Point>(Utils.EdgePoints(solution.Problem, solution.Problem.StageTopRight));
            HashSet<Point> backupGridPoints = new HashSet<Point>(Utils.SmallerGridPoints(solution.Problem));
            List<Musician> sortedMusicians =
                solution.Problem.Musicians.OrderByDescending(m => ScoreMusician(solution.Problem, m, edgePoints.First())).ToList();

            foreach (Musician m in sortedMusicians)
            {
                long bestScore = long.MinValue;
                Point bestPoint = Point.ORIGIN;
                foreach (Point p in edgePoints)
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
                edgePoints.Remove(bestPoint);

                if (edgePoints.Count == 0)
                {
                    edgePoints = backupGridPoints;
                }
            }
        }

        // Assumes no blocking!!
        private static long ScoreMusician(ProblemSpec problem, Musician musician, Point location)
        {
            long score = 0;
            for (int i = 0; i < problem.Attendees.Count; i++)
            {
                score += problem.PairScore(musician.Index, i, location, 1);
            }

            return score;
        }
    }
}
