using AI;
using Lib;

namespace Runner
{
    public class AICatalog
    {
        public delegate void AI(AIArgs args, LoggerBase logger);

        private static readonly Dictionary<string, AI> aiCatalog = new()
        {
            ["Example"] = ExampleAI.Solve
        };

        public static AI GetAI(string name)
        {
            return aiCatalog[name];
        }

        public static string[] Names()
        {
            return aiCatalog.Keys.ToArray();
        }
    }
}
