using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ICFP2023
{
    // NOTE!!!!!
    // This should be the immutable description of a problem.
    // Do not put mutable state related to the solving of a problem here!
    // There was a lot of pain in the HumanTetris year related to mutating the original figure.
    public class ProblemSpec
    {
        [JsonProperty("room_width")]
        public float RoomWidth { get; init; }

        [JsonProperty("room_height")]
        public float RoomHeight { get; init; }

        [JsonProperty("stage_width")]
        public float StageWidth { get; init; }

        [JsonProperty("stage_height")]
        public float StageHeight { get; init; }

        [JsonProperty("stage_bottom_left")]
        [JsonConverter(typeof(PointConverter))]
        public Point StageBottomLeft { get; init; }

        [JsonProperty("musicians")]
        [JsonConverter(typeof(MusicianConverter))]
        public List<Musician> Musicians { get; init; }

        [JsonProperty("attendees")]
        public List<Attendee> Attendees { get; init; }

        public ProblemSpec(
            float roomWidth,
            float roomHeight,
            float stageWidth,
            float stageHeight,
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
            return ReadJson(problemJson);
        }

        public static ProblemSpec ReadJson(string problemJson)
        {
            return JsonConvert.DeserializeObject<ProblemSpec>(problemJson);
        }

        public class PointConverter : JsonConverter<Point>
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

        public class MusicianConverter : JsonConverter<List<Musician>>
        {
            public override List<Musician> ReadJson(JsonReader reader, Type objectType, List<Musician> existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var array = JArray.Load(reader);
                return array.Select(a => new Musician(a.Value<int>())).ToList();
            }

            public override void WriteJson(JsonWriter writer, List<Musician> value, JsonSerializer serializer)
            {
                var array = new JArray(value.Select(m => m.Instrument));
                array.WriteTo(writer);
            }
        }
    }
}
