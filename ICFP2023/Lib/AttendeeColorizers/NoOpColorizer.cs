namespace ICFP2023.AttendeeColorizers
{
    internal static class NoOpColorizer
    {
        internal static Dictionary<Attendee, double> Evaluate(ProblemSpec problem)
        {
            // Everyone is red.
            return problem.Attendees.ToDictionary(a => a, _ => -1d);
        }
    }
}
