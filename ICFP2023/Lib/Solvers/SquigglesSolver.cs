namespace ICFP2023
{
    public class SquigglesSolver
    {
        public static void Solve(ProblemSpec problem, SharedSettings settings, UIAdapter ui)
        {
            Solution sol = new Solution(problem);

            // Get stage bounding box (margin of 10 in all direcctions)
            double stageLeft = problem.StageBottomLeft.X + 10;
            double stageRight = problem.StageBottomLeft.X + problem.StageWidth - 10;
            double stageTop = problem.StageBottomLeft.Y + problem.StageHeight - 10;
            double stageBottom = problem.StageBottomLeft.Y + 10;

            // Just start splaying out musicians left to right, top to bottom. Too lazy to sphere-pack.
            int musicianIndex = 0;
            for (double y = stageTop; y > stageBottom; y -= 10.0)
            {
                for (double x = stageLeft; x < stageRight; x += 10.0)
                {
                    if (musicianIndex == problem.Musicians.Count)
                    {
                        goto DonePlacing;
                    }

                    sol.SetPlacement(problem.Musicians[musicianIndex++], new Point(x, y));
                }
            }

            DonePlacing:
            ui.Render(sol);
        }
    }
}
