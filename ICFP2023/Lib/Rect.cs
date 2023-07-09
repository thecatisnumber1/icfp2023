using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class Rect
    {
        public Point BottomLeft { get; }
        public Point TopRight { get; }
        public Point BottomRight => new(Right, Bottom);
        public Point TopLeft => new(Left, Top);

        public double Bottom => BottomLeft.Y;
        public double Left => BottomLeft.X;
        public double Top => TopRight.Y;
        public double Right => TopRight.X;

        public Rect(Point lowerLeft, Point upperRight)
        {
            BottomLeft = lowerLeft;
            TopRight = upperRight;
        }
        public Rect(Point lowerLeft, double width, double height)
            : this(lowerLeft, lowerLeft + new Vec(width, height))
        { }

        public Rect Shrink(double amount)
        {
            Vec step = amount * new Vec(1, 1);
            return new Rect(BottomLeft + step, TopRight - step);
        }

        public List<Side> Sides =>
            new List<Side>()
            {
                new Side(TopLeft, TopRight),
                new Side(TopRight, BottomRight),
                new Side(BottomRight, BottomLeft),
                new Side(BottomLeft, TopLeft)
            };
    }

    public class Side
    {
        public readonly Point Left, Right;
        public readonly Vec Along, Outward;

        public double Length { get; }

        public Side(Point left, Point right)
        {
            Left = left;
            Right = right;
            Along = (right - left).Normalized();
            Outward = Along.RotateCCW();
            Length = right.Manhattan(left);
        }

        public Side Shrink(double amount)
        {
            Vec step = amount * Along;
            return new Side(Left + step, Right - step);
        }

        public double AlongComponent(Point p)
        {
            return Along.DotProduct(p - Left);
        }

        public double OutwardComponent(Point p)
        {
            return Outward.DotProduct(p - Left);
        }

        public Point Project(Point p)
        {
            return Left + AlongComponent(p) * Along;
        }

        public Side Translate(Vec v)
        {
            return new Side(Left + v, Right + v);
        }
    }
}
