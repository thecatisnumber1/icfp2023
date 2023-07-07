using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaMusic
{
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

    public class Problem
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

        public static Problem LoadProblem(int problemNum)
        {
            var problemJson = File.ReadAllText($@"..\..\..\..\problems/problem-{problemNum}.json");
            return JsonConvert.DeserializeObject<Problem>(problemJson);
        }

        public long ComputeScore()
        {
            long score = 0;
            foreach (var musician in Musicians)
            {
                foreach (var attendee in Attendees)
                {
                    // Determine if blocked
                    bool blocked = false;
                    foreach (var blockingMusician in Musicians)
                    {
                        if (blockingMusician == musician)
                        {
                            continue;
                        }

                        if (IsMusicianBlocked(attendee.Location, musician, blockingMusician))
                        {
                            blocked = true;
                            break;
                        }
                    }

                    if (blocked)
                    {
                        continue;
                    }

                    score += (long)Math.Ceiling(1000000 * attendee.Tastes[musician.Instrument] / attendee.Location.DistSq(musician.Location));
                }
            }

            return score;
        }

        public Problem Copy()
        {
            var newProblem = new Problem
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

        private bool IsMusicianBlocked(Point attendee, Musician musician, Musician blockingMusician)
        {
            const float blockRadius = 5.0f;

            // Calculate the vectors and the scalar projections
            var da = attendee - musician.Location;
            var db = blockingMusician.Location - musician.Location;
            var dot = VectorDotProduct(da, db);
            var len_sq = VectorDotProduct(db, db);

            // Find the point on the line (musician -> attendee) that is closest to the blocking musician
            var t = Math.Max(0, Math.Min(len_sq, dot)) / (len_sq == 0 ? 1 : len_sq);
            var projection = musician.Location + new Vec(t * db.X, t * db.Y);

            // If this point is within the blocking radius, the musician is blocked
            var dp = blockingMusician.Location - projection;
            return VectorDotProduct(dp, dp) <= blockRadius * blockRadius;
        }

        private float VectorDotProduct(Vec a, Vec b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

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
            return array.Select(a => new Musician { Instrument = a.Value<int>(), Location = Point.ORIGIN }).ToList();
        }

        public override void WriteJson(JsonWriter writer, List<Musician> value, JsonSerializer serializer)
        {
            var array = new JArray(value.Select(m => m.Instrument));
            array.WriteTo(writer);
        }
    }

}
