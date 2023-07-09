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

        public List<Side> Sides()
        {
            return new List<Side>()
            {
                new Side(TopLeft, TopRight),
                new Side(TopRight, BottomRight),
                new Side(BottomRight, BottomLeft),
                new Side(BottomLeft, TopLeft)
            };
        }
    }

    public class Side
    {
        public readonly Point Left, Right;
        public readonly Vec along, outward;
        public Side(Point left, Point right)
        {
            Left = left;
            Right = right;
            along = right - left;
            outward = along.RotateCCW();
        }
    }
}
