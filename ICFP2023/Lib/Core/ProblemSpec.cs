using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
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
        public string ProblemName { get; set; }
        public int ProblemNumber => int.Parse(ProblemName.Substring(ProblemName.IndexOf('-') + 1));

        public Point StageBottomRight => StageBottomLeft + StageWidth * Vec.EAST;

        public Point StageTopLeft => StageBottomLeft + StageHeight * Vec.NORTH;

        public Point StageTopRight => StageBottomLeft + StageWidth * Vec.EAST + StageHeight * Vec.NORTH;

        public double StageBottom => StageBottomLeft.Y;

        public double StageTop => StageTopLeft.Y;

        public double StageLeft => StageBottomLeft.X;

        public double StageRight => StageBottomRight.X;

        // 1 through 55 = Lightning round. 56 through 90 = Pillars + Playing Together
        public ProblemExtensions Extensions => ProblemNumber < 56 ? ProblemExtensions.None : (ProblemExtensions.Pillars | ProblemExtensions.PlayingTogether);

        public bool UsePlayingTogetherScoring => Extensions.HasFlag(ProblemExtensions.PlayingTogether);

        // Usually problems are numbered but sometimes folks add their own test problems so this takes a string.
        public static ProblemSpec Read(string problemName)
        {
            var problemJson = FileUtil.Read($@"problems/{problemName}.json");
            var problem = ReadJson(problemJson);
            problem.ProblemName = problemName;
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

        public long PairScore(int musicianIndex, int attendeeIndex, Point location, double playingTogetherBonus)
        {
            double pairScore = 1000000 * Attendees[attendeeIndex].Tastes[Musicians[musicianIndex].Instrument] /
                Attendees[attendeeIndex].Location.DistSq(location);
            if (UsePlayingTogetherScoring)
            {
                pairScore *= playingTogetherBonus;
            }

            return (long)Math.Ceiling(pairScore);
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
    }
}
