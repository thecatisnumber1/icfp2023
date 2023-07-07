using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ICFP2023
{
    public class SolutionManager
    {
        public readonly string Name;
        private readonly Dictionary<string, BestKeeper> Keepers;
        private readonly string DirName;

        public SolutionManager(string name)
        {
            Name = name;
            Keepers = new Dictionary<string, BestKeeper>();
            DirName = Path.Join(FileUtil.PathFor("solutions"), name);
            if (!Directory.Exists(DirName)) {
                Directory.CreateDirectory(DirName);
            }
        }

        public bool Offer(Solution solution)
        {
            string key = GenerateKey(solution.Problem, solution);
            LoadFile(solution.Problem, key, DirName);
            if (!Keepers[key].Offer(solution))
            {
                return false;
            }

            File.WriteAllText(FileNameForKey(key, DirName), solution.WriteJson());
            return true;
        }

        public Solution GetBest(ProblemSpec problem)
        {
            string key = GenerateKey(problem);
            LoadOverallBest(problem);
            if (Keepers[key].HasBest)
            {
                return Keepers[key].Best;
            }
            else
            {
                return null;
            }
        }

        private void LoadOverallBest(ProblemSpec problem)
        {
            var root = Directory.GetParent(DirName);
            if (root == null)
            {
                throw new Exception("Couldn't get root solutions directory.");
            }

            foreach (var dir in root.GetDirectories())
            {
                string key = GenerateKey(problem);
                LoadFile(problem, key, dir.FullName);
            }
        }

        private void LoadFile(ProblemSpec problem, string key, string path)
        {
            if (!Keepers.ContainsKey(key))
            {
                Keepers.Add(key, new BestKeeper(problem));
            }

            string filePath = "";
            if (File.Exists(FileNameForKey(key, path)))
            {
                filePath = FileNameForKey(key, path);
            }
            else if (File.Exists(FileNameForKey(problem.ProblemName, path))) 
            {
                filePath = FileNameForKey(problem.ProblemName, path); // HACKS - getting best from best directory
            }
            else
            {
                return;
            }

            var solution = Solution.Read(filePath, problem);
            Keepers[key].Offer(solution);
        }

        private string FileNameForKey(string key, string path)
        {
            return Path.Join(path, $"{key}.json");
        }

        private string GenerateKey(ProblemSpec problem, Solution solution = null)
        {
            return problem.ProblemName;
        }
    }
}
