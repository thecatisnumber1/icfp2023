namespace ICFP2023
{
    public class SquigglesSolver
    {
        public static Solution Solve(ProblemSpec problem, SharedSettings settings, UIAdapter ui)
        {
            for (int musicianCount = 9; musicianCount <= 9; musicianCount += 2)
            {
                problem.Musicians.Clear();
                for (int i = 0; i < musicianCount; i++)
                {
                    problem.Musicians.Add(new Musician(i, 0));
                }

                Console.WriteLine($"Musicians: {musicianCount}");
                Solution lineSolution = LinePlacer(problem);
                lineSolution.InitializeScore();
                Console.WriteLine($"Line score: {lineSolution.ScoreCache}");

                Solution lensSolution = LensPlacer(problem);
                lensSolution.InitializeScore();
                Console.WriteLine($"Lens score: {lensSolution.ScoreCache}");
                ui.Render(lensSolution);
            }

            return null;
        }

        public static Solution LensPlacer(ProblemSpec problem)
        {
            Solution solution = new Solution(problem);
            List<Point> lens = LensCrafter.CraftLens(problem.Attendees[0].Location, problem.Stage.Sides[0], problem.Musicians.Count / 2);
            for (int i = 0; i < lens.Count; i++)
            {
                solution.SetPlacement(problem.Musicians[i], lens[i]);
            }

            return solution;
        }

        public static Solution LinePlacer(ProblemSpec problem)
        {
            Solution solution = new Solution(problem);
            Point current = problem.Stage.TopLeft.Mid(problem.Stage.TopRight) - problem.Stage.Sides[0].Outward * 10;
            double spanLength = 10 * (problem.Musicians.Count - 1);
            current -= spanLength / 2 * problem.Stage.Sides[0].Along;
            Vec step = problem.Stage.Sides[0].Along * 10;
            for (int i = 0; i < problem.Musicians.Count; i++)
            {
                solution.SetPlacement(problem.Musicians[i], current);
                current += step;
            }

            return solution;
        }
    }
}
