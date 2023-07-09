using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public readonly struct Point : IEquatable<Point>
    {
        // TODO!!!!!!!!!
        // Make sure these names and types are appropriate for the the problem
        // Remove unnecesary stuff in the "SPECULATIVE" section
        // Add anything that's missing
        [JsonProperty("x")]
        public readonly double X;

        [JsonProperty("y")]
        public readonly double Y;

        public static readonly Point ORIGIN = new(0, 0);
        public static readonly Point INVALID = new(-1, -1);

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void Deconstruct(out double x, out double y)
        {
            x = X;
            y = Y;
        }

        public static Point operator +(Point p, Vec v)
            => new(p.X + v.X, p.Y + v.Y);

        public static Point operator +(Vec v, Point p) => p + v;

        public static Point operator -(Point p, Vec v) => p + -v;

        public static Vec operator -(Point p1, Point p2)
            => new(p1.X - p2.X, p1.Y - p2.Y);

        public static bool operator ==(Point p1, Point p2)
            => p1.X == p2.X && p1.Y == p2.Y;

        public static bool operator !=(Point p1, Point p2) => !(p1 == p2);

        public override bool Equals(object obj)
        {
            return obj is Point other && Equals(other);
        }

        public bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString() => $"({X:F2}, {Y:F2})";

        //SPECULATIVE

        // Todo: make sure int won't overflow here
        public readonly double DistSq(Point other) => (this - other).MagnitudeSq;

        public readonly double Dist(Point other) => (this - other).Magnitude;

        public readonly double Manhattan(Point other) => (this - other).Manhattan;

        public readonly Vec VecToRect(Point bl, Point tr) {
            double closeX;
            if (X < bl.X) {
                closeX = bl.X;
            }
            else if (X < tr.X) {
                closeX = X;
            }
            else {
                closeX = tr.X;
            }

            double closeY;
            if (Y < bl.Y) {
                closeY = bl.Y;
            }
            else if (Y < tr.Y) {
                closeY = Y;
            }
            else {
                closeY = tr.Y;
            }

            return (new Point(closeX, closeY) - this);
        }

        // Warning! not exact
        public readonly Point Mid(Point other) => new((X + other.X) / 2, (Y + other.Y) / 2);
    }
}
