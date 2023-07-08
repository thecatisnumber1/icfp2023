using ICFP2023.AttendeeColorizers;

namespace ICFP2023
{
    /// <summary>
    /// Determines values for attendees that get translated to colors
    /// </summary>
    /// <param name="problem">Problem to colorize</param>
    /// <returns>Map of Attendee to "values." Negative numbers are more red, Positive numbers are more green.</returns>
    public delegate Dictionary<Attendee, double> Colorizer(ProblemSpec problem);

    public static class AudienceColorizers
    {
        // Put your colorizers in here.
        private static Dictionary<string, Colorizer> colorizers = new()
        {
            ["NoOp"] = NoOpColorizer.Evaluate
        };

        public static Colorizer GetSolver(string algorithm)
        {
            return colorizers[algorithm];
        }

        public static string[] Names()
        {
            return colorizers.Keys.ToArray();
        }
    }
}
