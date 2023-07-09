using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class FixedPointMatcher
    {
        private static Random random = new Random();

        public static List<int> FindMatching(ProblemSpec problem, List<Point> slots, UIAdapter ui, int runtimeMs, int startingTemp = 5000, int endingTemp = 5000)
        {
            if (slots.Count < problem.Musicians.Count)
            {
                throw new Exception("Not enough slots");
            }

            Console.WriteLine($"Starting fixed point matcher with runtime {runtimeMs}ms, starting temp {startingTemp}, ending temp {endingTemp}");

            FixedPointSolution fixedPointSolution = new FixedPointSolution(problem, slots);
            InitializeSolution(problem, fixedPointSolution);
            long bestScore = fixedPointSolution.GetScore();
            List<int> bestSolution = fixedPointSolution.Slots.ToList();

            CoolingScheduler coolingScheduler = new CoolingScheduler(runtimeMs, startingTemp, endingTemp);
            int logDelayMs = 200;
            int lastLogTime = Environment.TickCount;
            double accepted = 0;
            double rejected = 0;
            while (!coolingScheduler.ICE_COLD())
            {
                if ((Environment.TickCount - lastLogTime) >= logDelayMs)
                {
                    Console.WriteLine($"T = {coolingScheduler.Temperature:F0}, B = {bestScore}, C = {fixedPointSolution.GetScore()}, % = {((accepted / (accepted + rejected)) * 100):F2}, R = {coolingScheduler.RemainingMs()}, {accepted + rejected}");
                    accepted = 0;
                    rejected = 0;
                    lastLogTime = Environment.TickCount;
                    coolingScheduler.Watch.Stop();
                    Solution uiSol = FixedPointSolution.MatchingToSolution(problem, fixedPointSolution.SlotLocations, fixedPointSolution.Slots);
                    ui.Render(uiSol);
                    coolingScheduler.Watch.Start();
                }

                Move move = GetNeighbor(fixedPointSolution);
                long currentCost = -fixedPointSolution.GetScore();
                move.Apply(fixedPointSolution);
                long neighborCost = -fixedPointSolution.GetScore();

                // Decide if we should accept the neighbour
                if (AcceptanceProbability(currentCost, neighborCost, coolingScheduler.Temperature) <= random.NextDouble())
                {
                    move.Undo(fixedPointSolution);
                    rejected++;
                }
                else
                {
                    accepted++;
                }

                // Keep track of the best solution found
                if (fixedPointSolution.GetScore() > bestScore)
                {
                    bestSolution = fixedPointSolution.Slots.ToList();
                    bestScore = fixedPointSolution.GetScore();
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

        private static Move GetNeighbor(FixedPointSolution solution)
        {
            // Pick two non-identical indexes
            int i0;
            int i1;
            do
            {
                i0 = random.Next(solution.Slots.Count);
                i1 = random.Next(solution.Slots.Count);
            } while (solution.Slots[i0] == solution.Slots[i1]);

            return new Move(i0, i1);
        }

        public static long ComputeCost(Solution solution)
        {
            return -solution.ScoreCache;
        }

        private static void InitializeSolution(ProblemSpec problem, FixedPointSolution solution)
        {
            foreach (Musician m in problem.Musicians)
            {
                int loc;
                do
                {
                    loc = random.Next(solution.Slots.Count);
                } while (solution.Slots[loc] != -1);

                solution.SetInstrument(loc, m.Instrument);
            }
        }

        record Move(int I0, int I1)
        {
            public void Apply(FixedPointSolution solution)
            {
                solution.Swap(I0, I1);
            }

            public void Undo(FixedPointSolution solution)
            {
                solution.Swap(I0, I1);
            }
        }
    }
}
