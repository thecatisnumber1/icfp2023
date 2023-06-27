using Lib;

namespace Runner
{
    public class RunLogger : LoggerBase
    {
        public override void Break(bool immediate)
        {
            // Do nothing;
        }

        public override void LogMessage(string logString)
        {
            Console.WriteLine(logString);
        }

        public override void LogStatusMessage(string logString)
        {
            Console.WriteLine(logString);
        }

        public override void LogError(string logString)
        {
            Console.Error.WriteLine(logString);
        }
    }
}
