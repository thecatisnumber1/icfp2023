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
            double yAdjust = gridSize * Math.Sqrt(3) / 2;
            List<Point> gridPoints = new List<Point>();

            bool offset = false;
            for (double x = problem.StageBottomLeft.X + gridSize; x < problem.StageBottomLeft.X + problem.StageWidth - gridSize; x += gridSize)
            {
                for (double y = problem.StageBottomLeft.Y + gridSize + (offset ? yAdjust : 0); y < problem.StageBottomLeft.Y + problem.StageHeight - gridSize; y += gridSize * Math.Sqrt(3))
                {
                    gridPoints.Add(new Point(x, y));
                }

                offset = !offset;
            }

            return gridPoints;
        }
    }
}
