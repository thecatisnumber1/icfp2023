using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class Musician
    {
        public int Instrument { get; init; }

        public Musician(int instrument)
        {
            Instrument = instrument;
        }

        public override bool Equals(object other)
        {
            return other is Musician && Instrument == ((Musician)other).Instrument;
        }
    }
}
