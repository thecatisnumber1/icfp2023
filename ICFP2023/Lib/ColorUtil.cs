using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public static class ColorUtil
    {
        public static void ColorInstruments(UIAdapter ui, ProblemSpec problem)
        {
            var colors = MakeInstrumentColors(problem);
            foreach (var m in problem.Musicians)
            {
                ui.SetMusicianColor(m.Index, colors[m.Instrument]);
            }
        }

        // Picks evenly spaced colors for each instrument in the problem
        public static Dictionary<int, string> MakeInstrumentColors(ProblemSpec problem)
        {
            var instruments = problem.Musicians.Select(m => m.Instrument).Distinct().ToList();
            return MakeColors(instruments);
        }

        // Picks evenly spaced colors for each of the given things
        public static Dictionary<T, string> MakeColors<T>(IEnumerable<T> toColor)
        {
            Dictionary<T, string> colors = new();
            int count = toColor.Count();

            int i = 0;
            foreach (T key in toColor)
            {
                colors[key] = ToRGB(i / (double)count, 1.0, 1.0);
                i++;
            }

            return colors;
        }

        public static string ToRGB(double h, double s, double v)
        {
            int hi = (int)(h * 6);
            double f = h * 6 - hi;
            double p = v * (1 - s);
            double q = v * (1 - f * s);
            double t = v * (1 - (1 - f) * s);

            var (r, g, b) = hi switch
            {
                0 => (v, t, p),
                1 => (q, v, p),
                2 => (p, v, t),
                3 => (p, q, v),
                4 => (t, p, v),
                _ => (v, p, q),
            };

            return $"#{(byte)(r * 255):X2}{(byte)(g * 255):X2}{(byte)(b * 255):X2}";
        }
    }
}
