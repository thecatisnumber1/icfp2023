using ICFP2023;
using System.Diagnostics;
using System.IO.Pipes;

namespace ICFP2023
{

    public class BadAnnealingSolver
    {
        private static Random random = new Random();

        public static Solution Solve(ProblemSpec problem, SharedSettings settings, UIAdapter ui)
        {
            Solution solution = new Solution(problem, Point.INVALID);

            InitialPlacement(solution, problem);
            solution.InitializeScore();

            var startScore = solution.NScoreFull();
            Console.Error.WriteLine($"Starting score {startScore}");

            long initTemp = Math.Max(startScore, 10000000);
            var best = Anneal(solution, ComputeCost, 60000, initTemp);
            best.InitializeScore(true);

            ui.Render(best);
            return best;
        }

        public static void InitialPlacement(Solution solution, ProblemSpec problem)
        {
            var p = solution.Problem;
            Console.Error.WriteLine("Finding initial placement");
            HashSet<Point> used = new HashSet<Point>();
            foreach (var m in problem.Musicians)
            {
                solution.SetPlacement(m,
                    new Point(
                        random.NextDouble() * (p.StageWidth - 20) + p.StageLeft + 10,
                        random.NextDouble() * (p.StageHeight - 20) + p.StageBottom + 10
                        ));
                // Point hot;
                // do
                // {
                //     hot = problem.Hottest(m, used);
                //     used.Add(hot);
                // } while (!solution.SetPlacement(m, hot, true));
            }
        }

        public static Solution Anneal(Solution solution, Heuristic heuristic, int runtimeMs=60000, long startingTemp = 5000, int endingTemp = 1)
        {
            Console.WriteLine($"Starting annealing solver with runtime {runtimeMs}ms, starting temp {startingTemp}, ending temp {endingTemp}");

            int logDelayMs = 1000;
            int lastLogTime = Environment.TickCount;
            double accepted = 0;
            double rejected = 0;
            double totalmoves = 0;

            Solution currentSolution = solution.Copy();
            Solution bestSolution = solution.Copy();

            currentSolution.Render(true);

            CoolingScheduler coolingScheduler = new CoolingScheduler(runtimeMs, startingTemp, endingTemp);
            while (!coolingScheduler.ICE_COLD())
            {
                if ((Environment.TickCount - lastLogTime) >= logDelayMs)
                {
                    totalmoves += accepted + rejected;
                    Console.Error.WriteLine($"{solution.Problem.ProblemNumber,4:N0}  T = {coolingScheduler.Temperature,12:F0}, {coolingScheduler.TempLog,12:F6}, B = {heuristic(bestSolution),16:N0}, C = {heuristic(currentSolution),16:N0}, % = {((accepted / (accepted + rejected)) * 100),7:F2}, R = {coolingScheduler.RemainingMs(),10:N0}, {(accepted + rejected),9:N0} {totalmoves,10:N0}");
                    accepted = 0;
                    rejected = 0;
                    lastLogTime = Environment.TickCount;
                    // currentSolution.Render();
                }

                for (var n = 0; n < 1000; n++) {
                    Move move;
                    bool issnap = false;
                    if (random.NextDouble() <= .25) {
                        move = GetSwap(currentSolution, coolingScheduler.Temperature);
                    } else if (random.NextDouble() <= .30) {
                        move = GetSnap(currentSolution, coolingScheduler.Temperature);
                        // issnap = true;
                    } else {
                        move = GetWalk(currentSolution, coolingScheduler.Temperature);
                    }

                    double currentCost = heuristic(currentSolution);
                    move.Apply(currentSolution);
                    double neighborCost = heuristic(currentSolution);

                    if (issnap){
                        Console.WriteLine(move);
                        Console.WriteLine(currentSolution.Placements[((MoveWalk)move).M0]);
                        var o = currentSolution.MusicianOverlaps(((MoveWalk)move).M0, currentSolution.Placements[((MoveWalk)move).M0]);
                        Console.WriteLine(o);
                        if (o >= 0) {
                            Console.WriteLine(currentSolution.Placements[o]);
                        }
                        currentSolution.IsValid();
                    }

                    // Decide if we should accept the neighbour
                    var acceptance = AcceptanceProbability(currentCost, neighborCost, coolingScheduler.Temperature);
                    if (acceptance <= random.NextDouble())
                    {
                        move.Undo(currentSolution);
                        rejected++;
                    }
                    else
                    {
                        accepted++;
                        // Console.WriteLine($"{coolingScheduler.Temperature:F0} {currentCost:N0} {(currentCost - neighborCost),16:N0} {acceptance:F2} {move}");
                    }
                    // Console.Error.WriteLine($"\t\t\t\t\t\t\t\t\t\t\t{string.Join(", ", currentSolution.Placements)}");

                    // Keep track of the best solution found
                    if (heuristic(currentSolution) < heuristic(bestSolution))
                    {
                        bestSolution = currentSolution.Copy();
                    }
                }

                currentSolution = bestSolution.Copy();

                coolingScheduler.AdvanceTemperature(accepted / (accepted + rejected));
            }

            Console.Error.WriteLine($"{solution.Problem.ProblemNumber,4:N0}  T = {coolingScheduler.Temperature,12:F0}, {coolingScheduler.TempLog,12:F6}, B = {heuristic(bestSolution),16:N0}, C = {heuristic(currentSolution),16:N0}, % = {((accepted / (accepted + rejected)) * 100),7:F2}, R = {coolingScheduler.RemainingMs(),10:N0}, {(accepted + rejected),9:N0} {totalmoves,10:N0}");

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

        private static Move GetSwap(Solution solution, double temp)
        {
            // Pick two non-identical indexes
            int m0;
            int m1;
            do
            {
                m0 = random.Next(solution.Placements.Count);
                m1 = random.Next(solution.Placements.Count);
            } while (solution.Problem.Musicians[m0].Instrument == solution.Problem.Musicians[m1].Instrument && solution.Problem.InstrumentCount > 1);

            return new MoveSwap(m0, m1);
        }

        public static double Pareto(double max=100, double min = 0) {
            double u = random.NextDouble();
            double t = -Math.Log(u) / 1.0;
            double increment = (max - min) / 6.0;
            return Math.Min(min + (t * increment), max);
        }

        public static double RandExp(double max, double min=0.0, double lambda=1.0)
        {
            double range = max - min;
            double x;

            do
            {
                double u = random.NextDouble();
                x = -Math.Log(1 - u) / lambda;  // standard exponential distribution
            }
            while (x > range);  // retry if outside the desired range

            return x + min;  // shift to desired range
        }

        private static Move GetSnap(Solution solution, double temp)
        {
            var p = solution.Problem;

            var m = p.Musicians[random.Next(solution.Placements.Count)];

            var toptries = 100;
            while (toptries-- > 0 ) {
                var attendee = p.Attendees[p.Strongest[m.Instrument, (int)RandExp(Math.Min(1000, p.Strongest.GetLength(1)-1))]];
                var dir = attendee.Location.VecToRect(p.StageFenceBottomLeft, p.StageFenceTopRight);
                var closest = attendee.Location + dir;

                var pos = random.NextDouble() < .5 ? -1 : 1;
                var overlap = solution.MusicianOverlaps(m.Index, closest);
                var tries = 10;
                while (overlap >= 0 && tries-- > 0) {
                    if (closest.X == p.StageFenceLeft || closest.X == p.StageFenceRight)
                    {
                        closest = new Point(closest.X, solution.Placements[overlap].Y + pos * (float)(10 + RandExp(2.0, 0.0, 2.0)));
                    }
                    else
                    {
                        closest = new Point(solution.Placements[overlap].X + pos * (float)(10 + RandExp(2.0, 0.0, 2.0)), closest.Y);
                    }

                    // Don't go outside the bounds
                    if (closest.X < solution.Problem.StageFenceLeft ||
                        closest.X > solution.Problem.StageFenceRight ||
                        closest.Y < solution.Problem.StageFenceBottom ||
                        closest.Y > solution.Problem.StageFenceTop) {

                        // Couldn't snap. Give up.
                        closest = Point.INVALID;
                        break;
                    }

                    overlap = solution.MusicianOverlaps(m.Index, closest);
                }

                if (overlap >= 0 || closest == Point.INVALID) continue;

                var loc = solution.Placements[m.Index];
                var delta = closest - loc;

                // Create the move
                Move move = new MoveWalk(m.Index, delta);
                // Console.WriteLine($"Snapping {m.Instrument} to {ahead} for {attendee.Location}");
                return move;
            }

            // Couldn't snap. Give up.
            return GetWalk(solution, temp);
        }

        private static Move GetWalk(Solution solution, double temp)
        {
            int musicianIndex;
            Vec delta;
            while (true)
            {
                musicianIndex = random.Next(solution.Placements.Count);

                delta = new Vec(
                    // RandExp(solution.Problem.StageWidth - 20, 0.0, 0.25) * (random.NextDouble() < 0.5 ? -1 : 1),
//                     RandExp(solution.Problem.StageHeight - 20, 0.0, 0.25) * (random.NextDouble() < 0.5 ? -1 : 1)
                    (random.NextDouble() - 0.50) * Math.Min(100, (solution.Problem.StageWidth - 20)),
                    (random.NextDouble() - 0.50) * Math.Min(100, (solution.Problem.StageHeight- 20))
                );

                if (delta.MagnitudeSq == 0) continue;

                var loc = solution.Placements[musicianIndex];
                var ahead = loc + delta;

                // Don't go outside the bounds
                if (ahead.X < solution.Problem.StageFenceLeft) continue;
                if (ahead.X > solution.Problem.StageFenceRight) continue;
                if (ahead.Y < solution.Problem.StageFenceBottom) continue;
                if (ahead.Y > solution.Problem.StageFenceTop) continue;

                // Towards the end, don't allow overlap moves
                if (solution.MusicianOverlaps(musicianIndex, ahead) >= 0) {
                    continue;
                }

                // Create the move
                Move move = new MoveWalk(musicianIndex, delta);

                return move;
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
                    if (solution.Placements[musician.Index].DistSq(targetMusician) < 10.0 * 10.0)  // Check if distance is less than 10
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        interface Move {
            public void Apply(Solution solution);
            public void Undo(Solution solution);
        }

        record MoveSwap(int M0, int M1) : Move
        {
            public void Apply(Solution solution)
            {
                Swap(solution);
            }

            public void Undo(Solution solution)
            {
                Swap(solution);
            }

            private void Swap(Solution solution)
            {
                var musician0 = solution.Problem.Musicians[M0];
                var musician1 = solution.Problem.Musicians[M1];

                solution.SwapNoCache(musician0, musician1);

                solution.NScoreWithCache(M0);
                solution.NScoreWithCache(M1);
            }
        }

        record MoveWalk(int M0, Vec delta) : Move
        {
            public void Apply(Solution solution)
            {
                var musician = solution.Problem.Musicians[M0];
                var current = solution.GetPlacement(musician);
                solution.SetPlacement(musician, current + delta);
                solution.NScoreWithCache(M0);
            }

            public void Undo(Solution solution)
            {
                var musician = solution.Problem.Musicians[M0];
                var current = solution.GetPlacement(musician);
                solution.SetPlacement(musician, current - delta);
                solution.NScoreWithCache(M0);
            }
        }
    }
}
