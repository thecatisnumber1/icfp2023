using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    // NOTE!!!!!
    // This should be the immutable description of a problem.
    // Do not put mutable state related to the solving of a problem here!
    // There was a lot of pain in the HumanTetris year related to mutating the original figure.
    public class ProblemSpec
    {
        [JsonProperty("room_width")]
        public float RoomWidth { get; set; }

        [JsonProperty("room_height")]
        public float RoomHeight { get; set; }

        [JsonProperty("stage_width")]
        public float StageWidth { get; set; }

        [JsonProperty("stage_height")]
        public float StageHeight { get; set; }

        [JsonProperty("stage_bottom_left")]
        [JsonConverter(typeof(PointConverter))]
        public Point StageBottomLeft { get; set; }

        [JsonProperty("musicians")]
        [JsonConverter(typeof(MusicianConverter))]
        public List<Musician> Musicians { get; set; }

        [JsonProperty("attendees")]
        public List<Attendee> Attendees { get; set; }

        // Usually problems are numbered but sometimes folks add their own test problems so this takes a string.
        public static ProblemSpec Read(string problemName)
        {
            var problemJson = FileUtil.Read($@"problems/problem-{problemName}.json");
            return ReadJson(problemJson);
        }

        public static ProblemSpec ReadJson(string problemJson)
        {
            return JsonConvert.DeserializeObject<ProblemSpec>(problemJson);
        }

        public ProblemSpec Copy()
        {
            var newProblem = new ProblemSpec
            {
                RoomWidth = this.RoomWidth,
                RoomHeight = this.RoomHeight,
                StageWidth = this.StageWidth,
                StageHeight = this.StageHeight,
                StageBottomLeft = new Point(this.StageBottomLeft.X, this.StageBottomLeft.Y),
                Musicians = new List<Musician>(this.Musicians.Select(m => new Musician { Instrument = m.Instrument, Location = new Point(m.Location.X, m.Location.Y) })),
                Attendees = new List<Attendee>(this.Attendees.Select(a => new Attendee { Location = new Point(a.Location.X, a.Location.Y), Tastes = new List<float>(a.Tastes) })),
            };

            return newProblem;
        }

        public class Attendee
        {
            [JsonProperty("location")]
            public Point Location { get; set; }

            [JsonProperty("tastes")]
            public List<float> Tastes { get; set; }
        }

        public class Musician
        {
            public Point Location { get; set; }
            public int Instrument { get; set; }
        }

        private class PointConverter : JsonConverter<Point>
        {
            public override Point ReadJson(JsonReader reader, Type objectType, Point existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var array = JArray.Load(reader);
                return new Point(array[0].Value<float>(), array[1].Value<float>());
            }

            public override void WriteJson(JsonWriter writer, Point value, JsonSerializer serializer)
            {
                var array = new JArray { value.X, value.Y };
                array.WriteTo(writer);
            }
        }

        private class MusicianConverter : JsonConverter<List<Musician>>
        {
            public override List<Musician> ReadJson(JsonReader reader, Type objectType, List<Musician> existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var array = JArray.Load(reader);
                return array.Select(a => new Musician { Instrument = a.Value<int>(), Location = Point.ORIGIN }).ToList();
            }

            public override void WriteJson(JsonWriter writer, List<Musician> value, JsonSerializer serializer)
            {
                var array = new JArray(value.Select(m => m.Instrument));
                array.WriteTo(writer);
            }
        }
    }
}
