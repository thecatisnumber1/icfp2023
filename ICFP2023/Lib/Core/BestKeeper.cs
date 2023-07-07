using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class BestKeeper
    {
        public Solution Best { get; private set; }
        public long BestScore { get; private set; }

        public bool HasBest => Best != null;

        private ProblemSpec Problem;

        public BestKeeper(ProblemSpec problem)
        {
            Problem = problem;
            BestScore = long.MinValue;
        }

        public bool Offer(Solution solution)
        {
            if (!solution.IsValid())
            {
                return false;
            }

            long score = solution.InitializeScore();
            if (score > BestScore)
            {
                Best = solution.Copy();
                BestScore = score;
                return true;
            }

            return false;
        }
    }
}
