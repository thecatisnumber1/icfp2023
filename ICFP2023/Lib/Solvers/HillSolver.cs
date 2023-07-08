using ICFP2023;
using System.Diagnostics;
using System.IO.Pipes;

namespace ICFP2023
{
    //public delegate long Heuristic(Solution solution);

    public class HillSolver
    {
        private static readonly Random random = new Random();

        public static Solution Solve(Solution initialSolution, Heuristic heuristic, int runtimeMs, int startingTemp = 5000, int endingTemp = 1)
        {
            Console.WriteLine($"Starting hill solver with runtime {runtimeMs}ms, starting temp {startingTemp}, ending temp {endingTemp}");
            initialSolution.InitializeScore();
            Console.WriteLine($"Finished initializing score: {initialSolution.ScoreCache}");

            Solution currentSolution = initialSolution.Copy();
            while (true)
            {
                bool swapped = false;
                for (int i = 0; i < currentSolution.Placements.Count-1; ++i) {
                    for (int j = i+1; j < currentSolution.Placements.Count; ++j) {
                        Move move = new Move(i, j);
                        double currentCost = heuristic(currentSolution);
                        move.Apply(currentSolution);
                        if (heuristic(currentSolution) < currentCost)
                        {
                            swapped = true;
                        }
                        else
                        {
                            move.Undo(currentSolution);
                        }
                    }
                    Console.WriteLine($"Completed pass: {i} / {currentSolution.Placements.Count - 1}");
                }

                Console.WriteLine($"Current climbing score: {currentSolution.ScoreCache}");
                break;
                if (!swapped)
                {
                    break;
                }
            }

            Console.WriteLine($"Finished climbing score: {currentSolution.ScoreCache}");

            return currentSolution;
        }

        record Move(int M0, int M1)
        {
            public void Apply(Solution solution)
            {
                solution.Swap(M0, M1);
            }

            public void Undo(Solution solution)
            {
                solution.Swap(M1, M0);
            }
        }
    }
}
