using Lib;

namespace AI
{
    public class ExampleAI
    {
        public static void Solve(AIArgs args, LoggerBase logger)
        {
            logger.LogMessage("Hello World");
            logger.LogMessage($"Pass {args.Example} example arg");
        }
    }
}
