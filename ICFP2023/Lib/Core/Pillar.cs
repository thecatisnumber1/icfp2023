namespace ICFP2023;

public sealed record class Pillar(int Index, Point Center, double Radius)
{
    public bool Equals(Pillar? other)
    {
        return other?.Index == Index;
    }

    public override int GetHashCode()
    {
        return Index;
    }
}
