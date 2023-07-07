using ICFP2023;
using System.Diagnostics;
using System.IO.Pipes;

namespace ICFP2023
{
    public delegate long Heuristic(Solution solution);

    public class AnnealingSolver
    {
        private static readonly Random random = new Random();

        public static Solution Solve(Solution initialSolution, Heuristic heuristic, int runtimeMs, int startingTemp = 5000, int endingTemp = 1)
        {
            int logDelayMs = 200;
            int lastLogTime = Environment.TickCount;
            double accepted = 0;
            double rejected = 0;

            Solution currentSolution = initialSolution.Copy();
            Solution bestSolution = initialSolution.Copy();
            CoolingScheduler coolingScheduler = new CoolingScheduler(runtimeMs, startingTemp, endingTemp);
            while (!coolingScheduler.ICE_COLD())
            {
                if ((Environment.TickCount - lastLogTime) >= logDelayMs)
                {
                    Console.WriteLine($"T = {coolingScheduler.Temperature:F0}, B = {heuristic(bestSolution)}, C = {heuristic(currentSolution)}, % = {((accepted / (accepted + rejected)) * 100):F2}, R = {coolingScheduler.RemainingMs()}, {accepted + rejected}");
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
            return -solution.ComputeScore();
        }

        private static Move GetNeighbor(Solution solution)
        {
            int musicianIndex;
            Vec delta;
            while (true)
            {
                musicianIndex = random.Next(solution.Placements.Count);

                // Generate a random vector, scaled and translated to be between 0.25 and 2.0
                delta = new Vec((float)random.NextDouble() * 1.75f + 0.25f, (float)random.NextDouble() * 1.75f + 0.25f);

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
        }

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
                    if (solution.Placements[musician.Index].DistSq(targetMusician) < 10.0f * 10.0f)  // Check if distance is less than 10
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void RandomizeStartingState(Solution solution)
        {
            Random random = new Random();
            const float edgeDistance = 10.0f; // Distance musicians should be from the stage edges

            for (int i = 0; i < solution.Placements.Count; i++)
            {
                do
                {
                    solution.Placements[i] = new Point(
                        solution.Problem.StageBottomLeft.X + edgeDistance + (float)random.NextDouble() * (solution.Problem.StageWidth - 2 * edgeDistance),
                        solution.Problem.StageBottomLeft.Y + edgeDistance + (float)random.NextDouble() * (solution.Problem.StageHeight - 2 * edgeDistance));
                }
                while (IsTooClose(solution, i));
            }
        }

        record Move(int Index, Vec Delta)
        {
            public void Apply(Solution solution)
            {
                solution.Placements[Index] += Delta;
            }

            public void Undo(Solution solution)
            {
                solution.Placements[Index] -= Delta;
            }
        }
    }
}
