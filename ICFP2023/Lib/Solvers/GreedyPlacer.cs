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
            HashSet<Point> backupGridPoints = new HashSet<Point>(Utils.GridPoints(solution.Problem.Stage.Shrink(Utils.GRID_SIZE)));
            Dictionary<(int, Point), long> instrumentPointScores = new();

            var instruments = solution.Problem.Musicians.Select(m => m.Instrument).Distinct();

            foreach (int instrument in instruments)
            {
                foreach (Point p in edgePoints.Concat(backupGridPoints))
                {
                    long score = ScoreInstrumentAt(solution.Problem, instrument, p);
                    instrumentPointScores.Add((instrument, p), score);
                }
            }

            List<Musician> sortedMusicians =
                solution.Problem.Musicians.OrderByDescending(m => instrumentPointScores[(m.Instrument, edgePoints.First())]).ToList();

            foreach (Musician m in sortedMusicians)
            {
                long bestScore = long.MinValue;
                Point bestPoint = Point.ORIGIN;
                foreach (Point p in edgePoints)
                {
                    long score = instrumentPointScores[(m.Instrument, p)];
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
        private static long ScoreInstrumentAt(ProblemSpec problem, int instrument, Point location)
        {
            long score = 0;
            for (int i = 0; i < problem.Attendees.Count; i++)
            {
                score += problem.PairScore(instrument, i, location, 1);
            }

            return score;
        }
    }
}
