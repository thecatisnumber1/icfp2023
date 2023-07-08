using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class BadScore
    {
        private static Random random = new Random();

        public static Solution Solve(ProblemSpec problem, SharedSettings settings, UIAdapter ui)
        {
            problem.LoadMetaData();

            Solution solution = new Solution(problem, Point.INVALID);

            InitialPlacement(solution, problem);
            solution.NScoreFull();
            Console.Error.WriteLine($"Starting score {solution.NScoreCacheTotal}");

            Anneal(solution, ui);

            ui.Render(solution);
            return solution;
        }

        public static void InitialPlacement(Solution solution, ProblemSpec problem)
        {
            Console.Error.WriteLine("Finding initial placement");
            HashSet<Point> used = new HashSet<Point>();
            foreach (var m in problem.Musicians) {
                Point hot;
                do {
                    hot = problem.Hottest(m, used);
                    used.Add(hot);
                } while (!solution.SetPlacement(m, hot, true) || !solution.IsValid());
            }
        }

        public static void Anneal(Solution solution, UIAdapter ui)
        {
            Console.Error.WriteLine("Annealing");

            var edgePoints = solution.Placements;
            var unusedPoints = new List<Point>(solution.Placements);

            // while found improvement
            //   foreach possible swap
            //     try it
            solution.InitializeScore();
            bool foundImprovement = true;
            while (foundImprovement)
            {
                foundImprovement = false;
                for (int i = 0; i < edgePoints.Count; i++)
                {
                    for (int j = i + 1; j < edgePoints.Count; j++)
                    {
                        Point p0 = edgePoints[i];
                        Point p1 = edgePoints[j];

                        // Case 1: No musicians at either point
                        if (unusedPoints.Contains(p0) && unusedPoints.Contains(p1))
                        {
                            continue;
                        }

                        // Case 2: One musician at one point
                        //     Make sure to update unusedPoints
                        if (unusedPoints.Contains(p0) ^ unusedPoints.Contains(p1))
                        {
                            var (dest, source) = unusedPoints.Contains(p0) ? (p0, p1) : (p1, p0);

                            // Find the index of the musician at the old location
                            int mIndex = FindMusicianIndex(solution, source);

                            long oldScore = solution.ScoreCache;
                            solution.SetPlacement(solution.Problem.Musicians[mIndex], dest);
                            long newScore = solution.InitializeScore();

                            if (oldScore < newScore) // Improved!
                            {
                                Console.Error.WriteLine($"Found improvement: {oldScore} -> {newScore}");
                                foundImprovement = true;
                                unusedPoints.Add(source);
                                unusedPoints.Remove(dest);
                                ui.Render(solution);
                            }
                            else // Not better :(
                            {
                                solution.SetPlacement(solution.Problem.Musicians[mIndex], source);
                                solution.InitializeScore();

                                if (oldScore != solution.ScoreCache)
                                {
                                    ;
                                }
                            }

                            continue;
                        }

                        // Case 3: Two musicians at different points
                        //     If same instrument, continue (rather than swap)
                        //     Use Swap API to not have to call InitializeScore
                        int m0Index = FindMusicianIndex(solution, p0);
                        int m1Index = FindMusicianIndex(solution, p1);

                        if (solution.Problem.Musicians[m0Index].Instrument == solution.Problem.Musicians[m1Index].Instrument)
                        {
                            continue;
                        }

                        long preScore = solution.ScoreCache;
                        solution.Swap(m0Index, m1Index);
                        long postScore = solution.ScoreCache;
                        if (preScore < postScore) // Improved!
                        {
                            Console.Error.WriteLine($"Found improvement: {preScore} -> {postScore}");
                            foundImprovement = true;
                            ui.Render(solution);
                        }
                        else // Not better :(
                        {
                            solution.Swap(m0Index, m1Index);
                            if (preScore != solution.ScoreCache)
                            {
                                ;
                            }
                        }
                    }
                }
            }
        }

        private static int FindMusicianIndex(Solution solution, Point point)
        {
            for (int i = 0; i < solution.Problem.Musicians.Count; i++)
            {
                if (solution.Placements[i] == point)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
