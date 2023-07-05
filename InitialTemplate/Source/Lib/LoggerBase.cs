namespace Lib
{
    public abstract class LoggerBase
    {
        /// <summary>
        /// Pauses immediately until the logger returns
        /// </summary>
        /// <param name="immediate">If true, blocks in the Logger. Else blocks opportunistically.</param>
        /// <remarks>If false, the visualizer will only block if the user has selected the Pause button.</remarks>
        public abstract void Break(bool immediate = false);

        public abstract void LogStatusMessage(string logString);

        public abstract void LogMessage(string logString);

        public abstract void LogError(string logString);
    }
}