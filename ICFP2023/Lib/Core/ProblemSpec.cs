using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ICFP2023
{
    public class ProblemSpec
    {
        [JsonIgnore]
        public string ProblemName { get; private set; }

        [JsonProperty("room_width")]
        public double RoomWidth { get; init; }

        [JsonProperty("room_height")]
        public double RoomHeight { get; init; }

        [JsonProperty("stage_width")]
        public double StageWidth { get; init; }

        [JsonProperty("stage_height")]
        public double StageHeight { get; init; }

        [JsonProperty("stage_bottom_left")]
        [JsonConverter(typeof(PointConverter))]
        public Point StageBottomLeft { get; init; }

        public Point StageBottomRight => StageBottomLeft + StageWidth * Vec.EAST;

        public Point StageTopLeft => StageBottomLeft + StageHeight * Vec.NORTH;

        public Point StageTopRight => StageBottomLeft + StageWidth * Vec.EAST + StageHeight * Vec.NORTH;

        public double StageBottom => StageBottomLeft.Y;

        public double StageTop => StageTopLeft.Y;

        public double StageLeft => StageBottomLeft.X;

        public double StageRight => StageBottomRight.X;

        [JsonProperty("musicians")]
        [JsonConverter(typeof(MusicianConverter))]
        public List<Musician> Musicians { get; init; }

        [JsonProperty("attendees")]
        public List<Attendee> Attendees { get; init; }

        public ProblemSpec(
            double roomWidth,
            double roomHeight,
            double stageWidth,
            double stageHeight,
            Point stageBottomLeft,
            List<Musician> musicians,
            List<Attendee> attendees)
        {
            RoomWidth = roomWidth;
            RoomHeight = roomHeight;
            StageWidth = stageWidth;
            StageHeight = stageHeight;
            StageBottomLeft = stageBottomLeft;
            Musicians = musicians;
            Attendees = attendees;
        }

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
            return JsonConvert.DeserializeObject<ProblemSpec>(problemJson);
        }

        public long PairScore(int musicianIndex, int attendeeIndex, Point location)
        {
            return (long)Math.Ceiling(1000000 * Attendees[attendeeIndex].Tastes[Musicians[musicianIndex].Instrument] /
                Attendees[attendeeIndex].Location.DistSq(location));
        }

        private class PointConverter : JsonConverter<Point>
        {
            public override Point ReadJson(JsonReader reader, Type objectType, Point existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var array = JArray.Load(reader);
                return new Point(array[0].Value<double>(), array[1].Value<double>());
            }

            public override void WriteJson(JsonWriter writer, Point value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        private class MusicianConverter : JsonConverter<List<Musician>>
        {
            public override List<Musician> ReadJson(JsonReader reader, Type objectType, List<Musician> existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var array = JArray.Load(reader);
                return array.Select((a, i) => new Musician(i, a.Value<int>())).ToList();
            }

            public override void WriteJson(JsonWriter writer, List<Musician> value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
