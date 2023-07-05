using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class SharedSettings : SettingsBase
    {
        [ShortHand("n")]
        public string Name = "foo";

        [ShortHand("c")]
        public int Count = 42;

        [ShortHand("t")]
        public bool Turbo = true;

        public string NoShort = "example";
    }
}
