using System.Collections.Generic;

namespace ICFP2023;

public sealed record class Attendee(int Index, Point Location, List<double> Tastes)
{
    public bool Equals(Attendee? other)
    {
        return other?.Index == Index;
    }

    public override int GetHashCode()
    {
        return Index;
    }
}
