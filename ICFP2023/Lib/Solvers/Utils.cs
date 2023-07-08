using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class Utils
    {
        public static List<Point> GridPoints(ProblemSpec problem)
        {
            float gridSize = 10.0f;
            List<Point> gridPoints = new List<Point>();
            for (double x = problem.StageBottomLeft.X + gridSize; x < problem.StageBottomLeft.X + problem.StageWidth - gridSize; x += gridSize)
            {
                for (double y = problem.StageBottomLeft.Y + gridSize; y < problem.StageBottomLeft.Y + problem.StageHeight - gridSize; y += gridSize)
                {
                    gridPoints.Add(new Point(x, y));
                }
            }

            return gridPoints;
        }
    }
}
