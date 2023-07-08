
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text;
using System.Text.Json;
using System;
using System.Diagnostics;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Threading.Tasks;


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
            for (int i = 1; i <= 90; i++)
            {
                try
                {
                    Console.WriteLine($"Solving problem {i}");
                    Solution solution = new Solution(ProblemSpec.Read($"problem-{i}"));

                    // Call desired AI and then pass the result from it to HillClimber for optimization.
                    Solution best = LetsGetCrackin.FixedPointAnnealSolve(solution.Problem, new ConsoleSettings(), new DoNothingUIAdapter());
                    // best = HillSolver.Solve(best);

                    Console.WriteLine($"Score: {best.ScoreCache}");
                    if (best.ScoreCache == 0)
                    {
                        Console.WriteLine("Skipping submission of zero-score solution.");
                    }
                    else
                    {
                        SubmitSolution(best, i).Wait();
                    }


                    // Optimize our best solution with BestSolveOptimizer, and only submit if its result is better than the old score.
                    /*string bestSaveFile = $"best-solves/{solution.Problem.ProblemName}.json";
                    Solution oldBestSolution = Solution.Read(bestSaveFile, solution.Problem);
                    oldBestSolution.InitializeScore();

                    Solution newBestSolution = BestSolveOptimizer.Solve(solution.Problem, new ConsoleSettings(), new DoNothingUIAdapter());
                    if (newBestSolution.ScoreCache > oldBestSolution.ScoreCache)
                    {
                        Console.WriteLine($"New best score: {newBestSolution.ScoreCache} > {oldBestSolution.ScoreCache} ({newBestSolution.ScoreCache - oldBestSolution.ScoreCache})");
                        SubmitSolution(newBestSolution, i).Wait();
                    }*/
                    // Solution best = EdgeClimber.Solve(solution.Problem, new ConsoleSettings(), new DoNothingUIAdapter());
                    // Console.WriteLine($"Best score: {best.ScoreCache}");
                    // SubmitSolution(best, i).Wait();
                    // solution.Problem.LoadMetaData();

                    long[,,] power;
                    string filename = "heatmap-" + i + ".json";
                    if (File.Exists(filename))
                    {
                        // Read from file
                        string jsonData = File.ReadAllText(filename);
                        power = JsonConvert.DeserializeObject<long[,,]>(jsonData);
                    }
                    else
                    {
                        // Compute and write to file
                        power = solution.PrecomputeStagePower();
                        File.WriteAllText(filename, JsonConvert.SerializeObject(power));
                    }

                    long[,,] gradients;
                    string gradientPath = $"gradients-{i}.json";
                    if (File.Exists(gradientPath)) {
                        // Read from file
                        string jsonData = File.ReadAllText(gradientPath);
                        gradients = JsonConvert.DeserializeObject<long[,,]>(jsonData);

                    } else {
                        Console.WriteLine($"Computing gradients for problem {i}");
                        gradients = Solution.ComputeGradients(power);

                        Console.WriteLine($"Writing gradients for problem {i}");
                        File.WriteAllText(gradientPath, JsonConvert.SerializeObject(gradients));
                    }

                    int[,] strongest;
                    string strongestPath = $"strongest-{i}.json";
                    if (File.Exists(strongestPath))
                    {
                        // Read from file
                        string jsonData = File.ReadAllText(strongestPath);
                        strongest = JsonConvert.DeserializeObject<int[,]>(jsonData);

                    }
                    else
                    {
                        Console.WriteLine($"Computing strongest for problem {i}");
                        strongest = solution.Problem.StrongestAttendees();

                        Console.WriteLine($"Writing strongest for problem {i}");
                        File.WriteAllText(strongestPath, JsonConvert.SerializeObject(strongest));
                    }

                    // for (var j = 0; j < gradients.GetLength(0); j++) {
                    //     Heatmap(power, j, "power-" + i + "-" + j + ".png");
                    // }
                    // for (var j = 0; j < gradients.GetLength(0); j++)
                    // {
                    //     Heatmap(gradients, j, "gradients-" + i + "-" + j + ".png");
                    // }

                    // AnnealingSolver.GridBasedStartingState(solution);
                    // Solution best = AnnealingSolver.Solve(solution, AnnealingSolver.ComputeCost, 60000, 1000000);
                    // Console.WriteLine($"Best score: {best.ScoreCache}");
                    // SubmitSolution(best, i).Wait();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error on problem {i}: {e.Message}");
                    // break;
                }
            }
            // });
        }

        static void Heatmap(long[,,] power, int inst, string filename)
        {
            int width = power.GetLength(1);
            int height = power.GetLength(2);

            // Create a new image of the right size
            using var image = new Image<Rgba32>(width, height);

            // Calculate the minimum and maximum power value for scaling
            long minPower = long.MaxValue;
            long maxPower = long.MinValue;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    maxPower = Math.Max(maxPower, power[inst, i, j]);
                    minPower = Math.Min(minPower, power[inst, i, j]);
                }
            }

            long range = maxPower - minPower;

            // Set each pixel color based on the power value
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    // Scale the power value to a 0-255 range
                    int intensity = (int)(((power[inst, i, j] - minPower) / (double)range) * 255);
                    var color = power[inst, i, j] < 0 ? new Rgba32((byte)intensity, 0, 0) : new Rgba32(0, (byte)intensity, 0);

                    image[i, j] = color;
                }
            }
            image.Save(filename);
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
            // Prepare the submission
            var submission = new Submission
            {
                ProblemId = problemId,  // Assuming your Problem class has an Id field
                Contents = solution.WriteJson(),
            };

            // Convert the submission to a JSON string
            var submissionJson = System.Text.Json.JsonSerializer.Serialize(submission);

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