using System.Text.Json.Serialization;

namespace Lib.Draw
{
    public struct Point
    {
        [JsonInclude]
        public readonly int X;
        [JsonInclude]
        public readonly int Y;

        public static readonly Point ORIGIN = new(0, 0);

        [JsonConstructor]
        public Point(int x = 0, int y = 0)
        {
            X = x;
            Y = y;
        }

        public Point Clone()
        {
            return new Point(X, Y);
        }

        public Point Add(Point other)
        {
            return new Point(X + other.X, Y + other.Y);
        }

        public Point Subtract(Point other)
        {
            return new Point(X - other.X, Y - other.Y);
        }

        public int ManhattanDist(Point other)
        {
            return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
        }

        public override string ToString()
        {
            return $"[{X}, {Y}]";
        }

        public static bool operator ==(Point p1, Point p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y;
        }

        public static bool operator !=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }

        public override bool Equals(object? obj)
        {
            return obj != null && obj is Point p && this == p;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}
