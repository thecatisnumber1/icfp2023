namespace ICFP2023.AttendeeColorizers
{
    internal static class ScoreColorizer
    {
        internal static Dictionary<Attendee, double> Evaluate(Solution solution)
        {
            Dictionary<Attendee, double> result = new Dictionary<Attendee, double>();
            foreach (Attendee a in solution.Problem.Attendees)
            {
                result.Add(a, solution.GetScoreForAttendee(a.Index));
            }
            
            return result;
        }
    }
}
