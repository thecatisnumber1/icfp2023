using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class EdgeClimber
    {
        private static Random random = new Random();

        public static Solution Solve(ProblemSpec problem, SharedSettings settings, UIAdapter ui)
        {
            //Solution solution = GreedyPlacer.Solve(problem, settings, ui);
            Solution solution = new Solution(problem);
            Place(solution, ui);
            ui.Render(solution);
            return solution;
        }

        public static void Place(Solution solution, UIAdapter ui)
        {
            List<Point> edgePoints = Utils.EdgePoints(solution.Problem, solution.Problem.StageTopRight);
            HashSet<Point> unusedPoints = new HashSet<Point>(edgePoints);

            if (solution.Problem.Musicians.Count > edgePoints.Count)
            {
                return;
            }

            // Randomly put on the edges
            for (int i = 0; i < solution.Problem.Musicians.Count; i++)
            {
                Musician musician = solution.Problem.Musicians[i];
                Point point;
                do
                {
                    point = edgePoints[random.Next(edgePoints.Count)];
                } while (!unusedPoints.Contains(point));

                solution.SetPlacement(musician, point);
                unusedPoints.Remove(point);
            }

            // while found improvement
            //   foreach possible swap
            //     try it
            solution.InitializeScore();
            bool foundImprovement = true;
            while (foundImprovement)
            {
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
                                Console.WriteLine($"Found improvement: {oldScore} -> {newScore}");
                                foundImprovement = true;
                                unusedPoints.Add(source);
                                unusedPoints.Remove(dest);
                                ui.Render(solution);
                            }
                            else // Not better :(
                            {
                                solution.SetPlacement(solution.Problem.Musicians[mIndex], source);
                                solution.InitializeScore();
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
                            Console.WriteLine($"Found improvement: {preScore} -> {postScore}");
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
