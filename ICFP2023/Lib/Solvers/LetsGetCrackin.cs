using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023.Solvers
{
    public class LetsGetCrackin
    {
        public static Solution Solve(ProblemSpec problem, SharedSettings settings, UIAdapter ui)
        {
            Solution solution = new Solution(problem);
            ui.Render(solution);
            return AnnealingSolver.Solve(solution, AnnealingSolver.ComputeCost, 45000, 1000000);
        }
    }
}
