using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    // NOTE!!!!!
    // This should be the immutable description of a problem.
    // Do not put mutable state related to the solving of a problem here!
    // There was a lot of pain in the HumanTetris year related to mutating the original figure.
    public class ProblemSpec
    {
        public ProblemSpec()
        {

        }

        // Usually problems are numbered but sometimes folks add their own test problems so this takes a string.
        public static ProblemSpec Read(string problemName)
        {
            return new ProblemSpec();
        }
    }
}
