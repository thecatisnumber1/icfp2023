
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
            for (int problemNum = 1; problemNum <= 45; problemNum++)
            {
                Console.WriteLine($"Solving problem {problemNum}");
                Solution solution = new Solution(ProblemSpec.Read($"problem-{problemNum}"));
                AnnealingSolver.RandomizeStartingState(solution);
                long bestScore = solution.ComputeScore();
                Solution bestSolution = solution.Copy();
                for (int i = 0; i < 5; i++)
                {
                    AnnealingSolver.RandomizeStartingState(solution);
                    long score = solution.ComputeScore();
                    if (score > bestScore)
                    {
                        Console.WriteLine($"New best score: {score}");
                        bestScore = score;
                        bestSolution = solution.Copy();
                    }
                }

                Console.WriteLine($"Submitting score for {problemNum}, with score {bestScore}");
                SubmitSolution(bestSolution, problemNum).Wait();
            }
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