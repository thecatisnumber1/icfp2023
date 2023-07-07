using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class Attendee
    {
        public Point Location { get; init; }
        public List<float> Tastes { get; init; }

        public Attendee(float x, float y, List<float> Tastes)
        {
            this.Location = new(x, y);
            this.Tastes = Tastes;
        }

        public override bool Equals(object other)
        {
            return other is Attendee && Location.Equals(((Attendee)other).Location) && Tastes.SequenceEqual(((Attendee)other).Tastes);
        }
    }
}
