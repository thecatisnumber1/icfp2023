using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public interface UIAdapter
    {
        void Render(Solution solution);

        void SetMusicianColor(int index, string color);

        void SetAllMusicianColors(string[] colors);

        void ClearAllColors();

        void ClearMusicianColor(int index);

        bool ShouldHalt();
    }
}
