using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class Pillar
    {
        public Point Center { get; init; }
        public double Radius { get; init; }

        [JsonConstructor]
        private Pillar(double[] center, double radius)
            : this(new Point(center[0], center[1]), radius)
        {
        }

        public Pillar(Point center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        public override bool Equals(object other)
        {
            return other is Pillar && Center == ((Pillar)other).Center && Radius == ((Pillar)other).Radius;
        }
    }
}
