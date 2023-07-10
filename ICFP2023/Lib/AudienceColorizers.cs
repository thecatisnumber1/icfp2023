using ICFP2023.AttendeeColorizers;

namespace ICFP2023
{
    public static class AudienceColorizers
    {
        /// <summary>
        /// Determines values for attendees that get translated to colors
        /// </summary>
        /// <param name="solution">Problem to colorize</param>
        /// <returns>Map of Attendee to "values." Negative numbers are more red, Positive numbers are more green.</returns>
        public delegate Dictionary<Attendee, double> Colorizer(Solution solution);

        // Put your colorizers in here.
        private static Dictionary<string, Colorizer> colorizers = new()
        {
            ["ScoreColorizer"] = ScoreColorizer.Evaluate
        };

        public static Colorizer GetColorizer(string algorithm)
        {
            return colorizers[algorithm];
        }

        public static string[] Names()
        {
            return colorizers.Keys.ToArray();
        }
    }
}
