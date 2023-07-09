using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class Solvers
    {
        public delegate Solution Solver(ProblemSpec problem, SharedSettings settings, UIAdapter uiAdapter);

        // Put your solvers in here.
        private static Dictionary<string, Solver> solvers = new()
        {
            ["Squiggles"] = SquigglesSolver.Solve,
            ["GreedyPlacer"] = GreedyPlacer.Solve,
            ["EdgeClimber"] = EdgeClimber.Solve,
            ["BestSolveOptimizer"] = BestSolveOptimizer.Solve,
            ["LetsGetCrackin"] = LetsGetCrackin.Solve,
            ["FixedPointCraker"] = LetsGetCrackin.FixedPointAnnealSolve,
            ["BadAnnealer"] = BadAnnealingSolver.Solve,
        };

        public static Solver GetSolver(string algorithm)
        {
            return solvers[algorithm];
        }

        public static string[] Names()
        {
            return solvers.Keys.ToArray();
        }
    }
}
