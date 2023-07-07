using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class Musician
    {
        // Min distance from any stage edge or other musician
        public const double SOCIAL_DISTANCE = 10.0;

        // Area around a musician than blocks sound from others
        public const double BLOCKING_RADIUS = 5.0;

        public int Index { get; init; }
        public int Instrument { get; init; }

        public Musician(int index, int instrument)
        {
            Index = index;
            Instrument = instrument;
        }

        public override bool Equals(object other)
        {
            return other is Musician && Index == ((Musician)other).Index && Instrument == ((Musician)other).Instrument;
        }
    }
}
