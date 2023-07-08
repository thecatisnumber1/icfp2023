using ICFP2023;
using System.Diagnostics;
using System.IO.Pipes;

namespace ICFP2023
{

    public class BadAnnealingSolver
    {
        private static Random random = new Random();

        public static Solution Solve(ProblemSpec problem, SharedSettings settings, UIAdapter ui, int runtime=10000)
        {
            problem.LoadMetaData();

            Solution solution = new Solution(problem, Point.INVALID);
            InitialPlacement(solution, problem);

            solution.InitializeScore();
            var startScore = solution.NScoreFull();
            Console.Error.WriteLine($"Starting score {startScore}");

            long initTemp = startScore * 9 / 10;
            Anneal(solution, ComputeCost, runtime, (int)initTemp);

            ui.Render(solution);
            return solution;
        }

        public static void InitialPlacement(Solution solution, ProblemSpec problem)
        {
            Console.Error.WriteLine("Finding initial placement");
            HashSet<Point> used = new HashSet<Point>();
            foreach (var m in problem.Musicians)
            {
                Point hot;
                do
                {
                    hot = problem.Hottest(m, used);
                    used.Add(hot);
                } while (!solution.SetPlacement(m, hot, true) || !solution.IsValid());
            }
        }

        public static Solution Anneal(Solution solution, Heuristic heuristic, int runtimeMs=10000, int startingTemp = 5000, int endingTemp = 1)
        {
            Console.WriteLine($"Starting annealing solver with runtime {runtimeMs}ms, starting temp {startingTemp}, ending temp {endingTemp}");

            int logDelayMs = 200;
            int lastLogTime = Environment.TickCount;
            double accepted = 0;
            double rejected = 0;

            Solution currentSolution = solution.Copy();
            Solution bestSolution = solution.Copy();
            CoolingScheduler coolingScheduler = new CoolingScheduler(runtimeMs, startingTemp, endingTemp);
            while (!coolingScheduler.ICE_COLD())
            {
                if ((Environment.TickCount - lastLogTime) >= logDelayMs)
                {
                    Console.Error.WriteLine($"T = {coolingScheduler.Temperature:F0}, B = {heuristic(bestSolution)}, C = {heuristic(currentSolution)}, % = {((accepted / (accepted + rejected)) * 100):F2}, R = {coolingScheduler.RemainingMs()}, {accepted + rejected}");
                    accepted = 0;
                    rejected = 0;
                    lastLogTime = Environment.TickCount;
                }

                Move move = GetNeighbor(currentSolution);
                double currentCost = heuristic(currentSolution);
                move.Apply(currentSolution);
                double neighborCost = heuristic(currentSolution);

                // Decide if we should accept the neighbour
                if (AcceptanceProbability(currentCost, neighborCost, coolingScheduler.Temperature) <= random.NextDouble())
                {
                    move.Undo(currentSolution);
                    rejected++;
                }
                else
                {
                    accepted++;
                }

                // Keep track of the best solution found
                if (heuristic(currentSolution) < heuristic(bestSolution))
                {
                    bestSolution = currentSolution.Copy();
                }

                coolingScheduler.AdvanceTemperature();
            }

            return bestSolution;
        }

        private static double AcceptanceProbability(double currentEnergy, double newEnergy, double temperature)
        {
            // If the new solution is better, accept it
            if (newEnergy < currentEnergy)
            {
                return 1.0;
            }

            // If the new solution is worse, calculate an acceptance probability
            return Math.Exp((currentEnergy - newEnergy) / temperature);
        }

        public static long ComputeCost(Solution solution)
        {
            return -solution.NScoreWithCache();
        }

        private static Move GetNeighbor(Solution solution)
        {
            // Pick two non-identical indexes
            int m0;
            int m1;
            do
            {
                m0 = random.Next(solution.Placements.Count);
                m1 = random.Next(solution.Placements.Count);
            } while (solution.Problem.Musicians[m0].Instrument == solution.Problem.Musicians[m1].Instrument);

            return new Move(m0, m1);
        }

        // Previous version of GetNeighbor which would move points slightly.
        /*private static Move GetNeighbor(Solution solution)
        {
            int musicianIndex;
            Vec delta;
            while (true)
            {
                musicianIndex = random.Next(solution.Placements.Count);

                // Generate a random vector, scaled and translated to be between 0.25 and 2.0
                delta = new Vec(random.NextDouble() * 1.75 + 0.25, random.NextDouble() * 1.75 + 0.25);

                // Create the move
                Move move = new Move(musicianIndex, delta);

                // Apply the move
                move.Apply(solution);

                // If the move is valid, return it
                if (IsValidMove(solution, musicianIndex))
                {
                    move.Undo(solution);
                    return move;
                }

                // Otherwise, undo the move and try again
                move.Undo(solution);
            }
        }*/

        public static bool IsCurrentlyValid(Solution solution)
        {
            for (int i = 0; i < solution.Placements.Count; i++)
            {
                if (!IsValidMove(solution, i))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsValidMove(Solution solution, int musicianIndex)
        {
            Point movedMusicianLocation = solution.Placements[musicianIndex];
            // Calculate the upper right corner of the stage
            Point stageTopRight = new Point(
                solution.Problem.StageBottomLeft.X + solution.Problem.StageWidth,
                solution.Problem.StageBottomLeft.Y + solution.Problem.StageHeight);

            // Check if the moved musician is still on the stage
            if (movedMusicianLocation.X < solution.Problem.StageBottomLeft.X ||
                movedMusicianLocation.Y < solution.Problem.StageBottomLeft.Y ||
                movedMusicianLocation.X > stageTopRight.X ||
                movedMusicianLocation.Y > stageTopRight.Y)
            {
                return false;
            }

            // Check if the moved musician is too close to any other musician
            if (IsTooClose(solution, musicianIndex))
            {
                return false;
            }

            return true;
        }

        private static bool IsTooClose(Solution solution, int musicianIndex)
        {
            Point targetMusician = solution.Placements[musicianIndex];

            foreach (Musician musician in solution.Problem.Musicians)
            {
                if (solution.Placements[musician.Index] != targetMusician)
                {
                    // Calculate distance squared to avoid costly square root operation
                    if (solution.Placements[musician.Index].DistSq(targetMusician) < 10.0 * 10.0)  // Check if distance is less than 10
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        record Move(int M0, int M1)
        {
            public void Apply(Solution solution)
            {
                solution.Swap(M0, M1);
                solution.NScoreWithCache(M0);
                solution.NScoreWithCache(M1);
            }

            public void Undo(Solution solution)
            {
                solution.Swap(M1, M0);
                solution.NScoreWithCache(M1);
                solution.NScoreWithCache(M0);
            }
        }
    }
}
