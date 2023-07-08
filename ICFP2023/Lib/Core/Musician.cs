namespace ICFP2023;

public sealed record class Musician(int Index, int Instrument)
{
    // Min distance from any stage edge or other musician
    public const double SOCIAL_DISTANCE = 10.0;

    // Area around a musician than blocks sound from others
    public const double BLOCKING_RADIUS = 5.0;

    public bool Equals(Musician? other)
    {
        return other?.Index == Index;
    }

    public override int GetHashCode()
    {
        return Index;
    }
}
