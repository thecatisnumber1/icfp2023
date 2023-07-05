using System.Text.Json.Serialization;

namespace Lib.Draw
{
    public class Box
    {
        [JsonInclude]
        public readonly Point BottomLeft;

        [JsonInclude]
        public readonly Point TopRight;

        [JsonIgnore]
        public Point BottomRight => new(Right, Bottom);
        [JsonIgnore]
        public Point TopLeft => new(Left, Top);

        [JsonIgnore]
        public int Top => TopRight.Y;
        [JsonIgnore]
        public int Bottom => BottomLeft.Y;
        [JsonIgnore]
        public int Left => BottomLeft.X;
        [JsonIgnore]
        public int Right => TopRight.X;

        [JsonIgnore]
        public int Width => Right - Left;

        [JsonIgnore]
        public int Height => Top - Bottom;

        [JsonIgnore]
        public int Area => Width * Height;

        public Box(int x1, int y1, int x2, int y2)
        {
            int left = Math.Min(x1, x2);
            int bottom = Math.Min(y1, y2);
            BottomLeft = new Point(left, bottom);
            TopRight = new Point(x1 + x2 - left, y1 + y2 - bottom);
        }

        [JsonConstructor]
        public Box(Point bottomLeft, Point topRight)
        {
            BottomLeft = bottomLeft;
            TopRight = topRight;
        }

        public Box Shift(int x, int y)
        {
            return new Box(Left + x, Bottom + y, Right + x, Top + y);
        }

        public override string ToString()
        {
            return $"({BottomLeft}, {TopRight})";
        }

        public static bool operator ==(Box b1, Box b2)
        {
            return b1.BottomRight == b2.BottomRight &&
                   b1.TopRight == b2.TopRight;
        }

        public static bool operator !=(Box b1, Box b2)
        {
            return !(b1 == b2);
        }

        public override bool Equals(object? obj)
        {
            return obj != null && obj is Box b && this == b;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BottomLeft, BottomRight);
        }
    }
}
