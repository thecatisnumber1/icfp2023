
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text;
using System.Text.Json;
using System;

namespace ICFP2023
{
    class Program
    {
        static void Main(string[] args)
        {
            int problemNum = 1;
            Solution solution = new Solution(ProblemSpec.Read($"problem-{problemNum}"));
            RandomizeStartingState(solution);
            while (solution.ComputeScore() < 0)
            {
                Console.WriteLine($"Score was negative ({solution.ComputeScore()}). Trying again...");
                RandomizeStartingState(solution);
            }

            Console.WriteLine($"Submitting solution with score {solution.ComputeScore()}");
            SubmitSolution(solution, problemNum).Wait();
        }

        public static void RandomizeStartingState(Solution solution)
        {
            Random random = new Random();
            const float edgeDistance = 10.0f; // Distance musicians should be from the stage edges

            for (int i = 0; i < solution.Problem.Musicians.Count; i++)
            {
                do
                {
                    solution.Placements[i] = new Point(
                        solution.Problem.StageBottomLeft.X + edgeDistance + (float)random.NextDouble() * (solution.Problem.StageWidth - 2 * edgeDistance),
                        solution.Problem.StageBottomLeft.Y + edgeDistance + (float)random.NextDouble() * (solution.Problem.StageHeight - 2 * edgeDistance));
                }
                while (IsTooClose(solution, i));
            }
        }

        private static bool IsTooClose(Solution solution, int musicianIndex)
        {
            Point targetMusician = solution.Placements[musicianIndex];

            foreach (Musician musician in solution.Problem.Musicians)
            {
                if (solution.Placements[musician.Index] != targetMusician)
                {
                    // Calculate distance squared to avoid costly square root operation
                    if (solution.Placements[musician.Index].DistSq(targetMusician) < 10.0f * 10.0f)  // Check if distance is less than 10
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public class Submission
        {
            [JsonPropertyName("problem_id")]
            public int ProblemId { get; set; }

            [JsonPropertyName("contents")]
            public string Contents { get; set; }
        }

        public static async Task<string> SubmitSolution(Solution solution, int problemId)
        {
            // Prepare the placements
            var placements = solution.Placements.Select(m => new { x = m.X, y = m.Y }).ToList();

            // Convert the placements to a JSON string
            var placementsJson = JsonSerializer.Serialize(new { placements });

            // Prepare the submission
            var submission = new Submission
            {
                ProblemId = problemId,  // Assuming your Problem class has an Id field
                Contents = placementsJson
            };

            // Convert the submission to a JSON string
            var submissionJson = JsonSerializer.Serialize(submission);

            // Create an HTTP client
            var client = new HttpClient();

            // Set the Authorization header to 'Bearer <token>'
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer",
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1aWQiOiI2NDljYjJkMDhjNjg1MzEzZDFjNjBkM2UiLCJpYXQiOjE2ODg2OTQ5MzksImV4cCI6MTY5ODY5NDkzOX0.Lv5YBF7HmO4Cdt2oUmY4Znji4nxcCkIEQvCsHoutGEA");

            // Prepare the content of the request
            var content = new StringContent(submissionJson, Encoding.UTF8, "application/json");

            // Send the POST request to the API
            var response = await client.PostAsync("https://api.icfpcontest.com/submission", content);

            // Read the response content
            var responseContent = await response.Content.ReadAsStringAsync();

            return responseContent;
        }
    }
}