
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text;
using System.Text.Json;
using System;
using System.Diagnostics;

namespace ICFP2023
{
    class DoNothingUIAdapter : UIAdapter
    {
        public void ClearAllColors()
        {
        }

        public void ClearMusicianColor(int index)
        {
        }

        public void Render(Solution solution)
        {
        }

        public void SetMusicianColor(int index, string color)
        {
        }

        public bool ShouldHalt()
        {
            return false;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<int> toDoList = new List<int> { 12, 6 };
            foreach (int i in toDoList)
            {
                try
                {
                    Console.WriteLine($"Solving problem {i}");
                    Solution solution = new Solution(ProblemSpec.Read($"problem-{i}"));
                    Solution best = EdgeClimber.Solve(solution.Problem, new ConsoleSettings(), new DoNothingUIAdapter());
                    Console.WriteLine($"Best score: {best.ScoreCache}");
                    SubmitSolution(best, i).Wait();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error on problem {i}: {e.Message}");
                }
            }
        }

        private static void SubmitRandomSolutions()
        {
            HashSet<int> toDoList = new HashSet<int> { 5, 7, 18, 56 };
            foreach (int problemNum in toDoList)
            {
                Stopwatch sw = Stopwatch.StartNew();

                Console.WriteLine($"Solving problem {problemNum}");
                Solution solution = new Solution(ProblemSpec.Read($"problem-{problemNum}"));

                AnnealingSolver.GridBasedStartingState(solution);
                long bestScore = solution.InitializeScore();
                Solution bestSolution = solution.Copy();

                for (int i = 0; i < 5; i++)
                {
                    Stopwatch computeScoreSw = Stopwatch.StartNew();

                    AnnealingSolver.GridBasedStartingState(solution);
                    long score = solution.InitializeScore();

                    computeScoreSw.Stop();
                    Console.WriteLine($"ComputeScore time for iteration {i + 1}: {computeScoreSw.ElapsedMilliseconds}ms");

                    if (score > bestScore)
                    {
                        Console.WriteLine($"New best score: {score}");
                        bestScore = score;
                        bestSolution = solution.Copy();
                    }
                }

                Console.WriteLine($"Submitting score for {problemNum}, with score {bestScore}");

                sw.Stop();
                Console.WriteLine($"Total time for problem {problemNum}: {sw.ElapsedMilliseconds}ms");
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