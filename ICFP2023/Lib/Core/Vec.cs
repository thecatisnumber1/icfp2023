using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public readonly struct Vec
    {
        // TODO!!!!!!!!!
        // Make sure these names and types are appropriate for the the problem
        // Update (and maybe nename) the cardinal directions (The existing ones where written assuming a top left origin)
        // Make sure the rotate methods are correct for the coordinate system (they may be backward)
        // Remove unnecesary stuff in the "SPECULATIVE" section
        // Add anything that's missing
        public readonly float X, Y;

        public static readonly Vec ZERO = new(0, 0);
        public static readonly Vec WEST = new(-1, 0);
        public static readonly Vec EAST = -WEST;
        public static readonly Vec NORTH = new(0, -1);
        public static readonly Vec SOUTH = -NORTH;

        public static readonly Vec[] DIRECTIONS = new Vec[] { NORTH, EAST, SOUTH, WEST };

        public Vec(float x, float y)
        {
            X = x;
            Y = y;
        }

        public readonly void Deconstruct(out float x, out float y)
        {
            x = X;
            y = Y;
        }

        public static Vec operator +(Vec v1, Vec v2)
            => new(v1.X + v2.X, v1.Y + v2.Y);

        public static Vec operator -(Vec v) => new(-v.X, -v.Y);

        public static Vec operator -(Vec v1, Vec v2) => v1 + -v2;

        public static Vec operator *(int scale, Vec v)
            => new(scale * v.X, scale * v.Y);

        public static Vec operator /(Vec v, int scale)
            => new(v.X / scale, v.Y / scale);

        public static Vec operator *(Vec v, int scale) => scale * v;

        public static bool operator ==(Vec v1, Vec v2)
            => v1.X == v2.X && v1.Y == v2.Y;

        public static bool operator !=(Vec v1, Vec v2) => !(v1 == v2);

        public override readonly bool Equals(object obj) => obj is Vec v && this == v;

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;

                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();

                return hash;
            }
        }

        public override readonly string ToString() => $"<{X}, {Y}>";

        //SPECULATIVE
        // Todo: make sure int won't overflow here
        public readonly double MagnitudeSq => X * X + Y * Y;

        public readonly double Magnitude => Math.Sqrt(MagnitudeSq);

        public readonly float Manhattan => Math.Abs(X) + Math.Abs(Y);

        public Vec RotateClockwise()
        {
            return new Vec(-Y, X);
        }

        public Vec RotateCounterClockwise()
        {
            return new Vec(Y, -X);
        }

        public float DotProduct(Vec b)
        {
            return X * b.X + Y * b.Y;
        }
    }
}
