using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    // To get the names of the problems use:
    // ProblemCatalog.Instance.Names
    //
    // To get a ProblemSpec use:
    // ProblemCatalog.Instance.GetSpec(name)
    //
    // ProblemSpecs will be only be loaded once they are requested unless LoadAllSpecs is called
    //
    // Todo: Sometimes there are different categories of problems because of spec additions.
    //       It might be nice to be able to get names by category?
    public class ProblemCatalog
    {
        public readonly string[] Names;
        private readonly Dictionary<string, ProblemSpec> specs;

        private static ProblemCatalog instance = null;
        public static ProblemCatalog Instance => instance ??= new ProblemCatalog();

        public ProblemSpec GetSpec(string problemName) => specs[problemName] ??= ProblemSpec.Read(problemName);

        private static string[] DiscoverProblemNames()
        {
            // Note: Often problems are defined by a group files linked via naming convention so this will likely need to change
            //       Also it might be good to sort it so things show up in a good order in the UI.
            return FileUtil.GetFilesInDir("problems").Select(Path.GetFileNameWithoutExtension).ToArray();
        }

        public void LoadAllSpecs()
        {
            foreach (string name in Names)
            {
                GetSpec(name);
            }
        }

        public ProblemCatalog()
        {
            Names = DiscoverProblemNames();
            specs = new Dictionary<string, ProblemSpec>();
            foreach (string name in Names)
            {
                specs[name] = null;
            }
        }
    }
}
