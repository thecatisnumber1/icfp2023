using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class BestSolveOptimizer
    {
        public static Solution Solve(ProblemSpec problem, SharedSettings settings, UIAdapter ui)
        {
            string bestSaveFile = $"best-solves/{problem.ProblemName}.json";
            Solution solution = Solution.Read(bestSaveFile, problem);
            return HillSolver.Solve(solution, AnnealingSolver.ComputeCost, 45000, 1000000);
        }
    }
}