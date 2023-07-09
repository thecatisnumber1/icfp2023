using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ICFP2023
{
    [Flags]
    public enum ProblemExtensions {
        None = 0, // Lightning round
        Pillars = 1, // Extension 1
        PlayingTogether = 2, // Extension 2
    }

    public record class ProblemSpec(
        double RoomWidth,
        double RoomHeight,
        double StageWidth,
        double StageHeight,
        Point StageBottomLeft,
        List<Musician> Musicians,
        List<Attendee> Attendees,
        List<Pillar> Pillars
    )
    {
        // Public for testing
        public string ProblemName { get; private set; }
        public int ProblemNumber { get; private set; }

        public Point StageBottomRight => StageBottomLeft + StageWidth * Vec.EAST;

        public Point StageTopLeft => StageBottomLeft + StageHeight * Vec.NORTH;

        public Point StageTopRight => StageBottomLeft + StageWidth * Vec.EAST + StageHeight * Vec.NORTH;

        public double StageBottom => StageBottomLeft.Y;

        public double StageTop => StageTopLeft.Y;

        public double StageLeft => StageBottomLeft.X;

        public double StageRight => StageBottomRight.X;

        public Rect Stage => new Rect(StageBottomLeft, StageWidth, StageHeight);

        public ProblemExtensions Extensions { get; private set; }
        public bool UsePlayingTogetherScoring { get; private set; }

        public double StageFenceBottom => StageBottom + Musician.SOCIAL_DISTANCE;

        public double StageFenceTop => StageTop - Musician.SOCIAL_DISTANCE;

        public double StageFenceLeft => StageLeft + Musician.SOCIAL_DISTANCE;

        public double StageFenceRight => StageRight - Musician.SOCIAL_DISTANCE;


        // 1 through 55 = Lightning round. 56 through 90 = Pillars + Playing Together

        public void SetProblemName(string problemName)
        {
            ProblemName = problemName;
            ProblemNumber = int.Parse(ProblemName.Substring(ProblemName.IndexOf('-') + 1));
            // 1 through 55 = Lightning round. 56 through 90 = Pillars + Playing Together
            Extensions = ProblemNumber < 56 ? ProblemExtensions.None : (ProblemExtensions.Pillars | ProblemExtensions.PlayingTogether);
            UsePlayingTogetherScoring = Extensions.HasFlag(ProblemExtensions.PlayingTogether);
        }

        public int InstrumentCount
        {
            get
            {
                return Musicians.Max(x => x.Instrument) + 1;
            }
        }

        public long[,,] HeatMap { get; set; }
        public long[,,] Gradients { get; set; }
        public int[,] Strongest { get; set; }

        // Usually problems are numbered but sometimes folks add their own test problems so this takes a string.
        public static ProblemSpec Read(string problemName)
        {
            var problemJson = FileUtil.Read($@"problems/{problemName}.json");
            var problem = ReadJson(problemJson);
            problem.SetProblemName(problemName);
            return problem;
        }

        public static ProblemSpec ReadJson(string problemJson)
        {
            var raw = JsonConvert.DeserializeObject<RawProblem>(problemJson);

            List<Musician> musicians = new();
            List<Attendee> attendees = new();
            List<Pillar> pillars = new();

            int mi = 0;
            foreach (var instrument in raw.musicians)
            {
                musicians.Add(new(mi++, instrument));
            }

            int ai = 0;
            foreach (var attendee in raw.attendees)
            {
                attendees.Add(new(ai++, new(attendee.x, attendee.y), attendee.tastes.ToList()));
            }

            int pi = 0;
            foreach (var pillar in raw.pillars)
            {
                pillars.Add(new(pi++, new(pillar.center[0], pillar.center[1]), pillar.radius));
            }

            return new(
                raw.room_width,
                raw.room_height,
                raw.stage_width,
                raw.stage_height,
                new(raw.stage_bottom_left[0], raw.stage_bottom_left[1]),
                musicians,
                attendees,
                pillars
            );
        }

        public long PairScore(int instrument, int attendeeIndex, Point location, double playingTogetherBonus)
        {
            double pairScore = Math.Ceiling(1000000 * Attendees[attendeeIndex].Tastes[instrument] /
                Attendees[attendeeIndex].Location.DistSq(location));
            if (UsePlayingTogetherScoring)
            {
                pairScore *= playingTogetherBonus;
            }

            return (long)Math.Ceiling(pairScore);
        }

        public double PlayingTogetherBonus(Point from, IEnumerable<Point> toPoints)
        {
            if (!UsePlayingTogetherScoring)
            {
                return 1;
            }

            double q = 1;

            foreach (Point to in toPoints)
            {
                if (!from.Equals(to))
                {
                    q += 1 / from.Dist(to);
                }
            }

            return q;
        }

        private record class RawProblem(
            double room_width,
            double room_height,
            double stage_width,
            double stage_height,
            double[] stage_bottom_left,
            int[] musicians,
            RawAttendee[] attendees,
            RawPillar[] pillars
        );

        private record class RawAttendee(
            double x,
            double y,
            double[] tastes
        );

        private record class RawPillar(
            double[] center,
            double radius
        );

        public int[,] StrongestAttendees() {
            int[,] sorted = new int[InstrumentCount, Attendees.Count()];
            for (var i = 0; i < InstrumentCount; i++) {
                var attendees = Attendees
                    .Select((obj, index) => new { Object = obj, Index = index })
                    .OrderBy(a =>
                        -Math.Abs(1000000 * a.Object.Tastes[i] / a.Object.Location.VecToRect(StageBottomLeft, StageTopRight).MagnitudeSq)
                    )
                    .Select(item => item.Index)
                    .ToArray();
                for (var j = 0; j < attendees.Length; j++) sorted[i,j] = attendees[j];
            }
            return sorted;
        }

        public void LoadMetaData() {
            LoadMetaDataStrongest();
            LoadMetaDataGradients();
            LoadMetaDataHeatMap();
        }

        public void LoadMetaDataHeatMap() {
            string i = Regex.Replace(ProblemName, "[^0-9]", "");

            if (FileUtil.FileExists($"metadata/heatmap-{i}.json") && HeatMap == null)
            {
                Console.Error.WriteLine($"Loading Heatmap for {i}");
                HeatMap = JsonConvert.DeserializeObject<long[,,]>(FileUtil.Read($"metadata/heatmap-{i}.json"));
            }
        }

        public void LoadMetaDataGradients()
        {
            string i = Regex.Replace(ProblemName, "[^0-9]", "");

            if (FileUtil.FileExists($"metadata/gradients-{i}.json") && Gradients == null)
            {
                Console.Error.WriteLine($"Loading Gradients for {i}");
                Gradients = JsonConvert.DeserializeObject<long[,,]>(FileUtil.Read($"metadata/gradients-{i}.json"));
            }
        }

        public void LoadMetaDataStrongest()
        {
            string i = Regex.Replace(ProblemName, "[^0-9]", "");

            if (FileUtil.FileExists($"metadata/strongest-{i}.json"))
            {
                Console.Error.WriteLine($"Loading Strongest for {i}");
                Strongest = JsonConvert.DeserializeObject<int[,]>(FileUtil.Read($"metadata/strongest-{i}.json"));
            }
        }


        public Point Hottest(Musician m, HashSet<Point> ignore=null)
        {
            int maxX = 0;
            int maxY = 0;
            long maxH = Int64.MinValue;

            for (var x = 10; x < HeatMap.GetLength(1) - 10; x++)
            {
                for (var y = 10; y < HeatMap.GetLength(2) - 10; y++)
                {
                    if (maxH < HeatMap[m.Instrument, x, y]) {
                        if (ignore != null && ignore.Contains(new Point(StageLeft + x, StageBottom + y))) {
                            continue;
                        }

                        maxH = HeatMap[m.Instrument, x, y];
                        maxX = x;
                        maxY = y;
                    }
                }
            }

            return new Point(StageLeft + maxX, StageBottom + maxY);
        }
    }
}
