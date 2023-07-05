using System.Text.Json.Serialization;

namespace Lib.Draw
{
    public struct Color
    {
        [JsonInclude]
        public readonly byte R;

        [JsonInclude]
        public readonly byte G;

        [JsonInclude]
        public readonly byte B;

        [JsonInclude]
        public readonly byte A;

        public static readonly Color WHITE = new(255, 255, 255, 255);
        public static readonly Color BLACK = new(0, 0, 0, 255);
        public static readonly Color EMPTY = new(0, 0, 0, 0);

        [JsonConstructor]
        public Color(byte r = 0, byte g = 0, byte b = 0, byte a = 0)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color Clone()
        {
            return new Color(R, G, B, A);
        }

        public double Diff(Color other)
        {
            var rDist = (R - other.R) * (R - other.R);
            var gDist = (G - other.G) * (G - other.G);
            var bDist = (B - other.B) * (B - other.B);
            var aDist = (A - other.A) * (A - other.A);
            var distance = Math.Sqrt(rDist + gDist + bDist + aDist);
            return distance;
        }

        public override string ToString()
        {
            return $"[{R}, {G}, {B}, {A}]";
        }

        public static bool operator ==(Color c1, Color c2)
        {
            return c1.R == c2.R
                && c1.G == c2.G
                && c1.B == c2.B
                && c1.A == c2.A;
        }

        public static bool operator !=(Color c1, Color c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object? obj)
        {
            return obj != null && obj is Color c && this == c;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B, A);
        }
    }
}
