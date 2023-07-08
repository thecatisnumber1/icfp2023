using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class Utils
    {
        public static readonly double GRID_SIZE = 10.0;

        public static List<Point> GridPoints(ProblemSpec problem)
        {
            List<Point> gridPoints = new List<Point>();
            for (double x = problem.StageBottomLeft.X + GRID_SIZE; x < problem.StageBottomLeft.X + problem.StageWidth - GRID_SIZE; x += GRID_SIZE)
            {
                for (double y = problem.StageBottomLeft.Y + GRID_SIZE; y < problem.StageBottomLeft.Y + problem.StageHeight - GRID_SIZE; y += GRID_SIZE)
                {
                    gridPoints.Add(new Point(x, y));
                }
            }

            return gridPoints;
        }

        public static List<Point> SmallerGridPoints(ProblemSpec problem)
        {
            List<Point> gridPoints = new List<Point>();
            for (double x = problem.StageBottomLeft.X + GRID_SIZE * 2; x < problem.StageBottomLeft.X + problem.StageWidth - GRID_SIZE * 2; x += GRID_SIZE)
            {
                for (double y = problem.StageBottomLeft.Y + GRID_SIZE * 2; y < problem.StageBottomLeft.Y + problem.StageHeight - GRID_SIZE * 2; y += GRID_SIZE)
                {
                    gridPoints.Add(new Point(x, y));
                }
            }

            return gridPoints;
        }

        // Starting corner is assumed to be on the corner of the stage.  Use StageBottomLeft, StageBottomRight, etc.
        public static List<Point> EdgePoints(ProblemSpec problem, Point stageStartingCorner)
        {
            List<Point> edgePoints = new List<Point>();
            Vec vertical;
            double oppositeX;
            double oppositeY;
            if (stageStartingCorner.Y == problem.StageBottom)
            {
                vertical = GRID_SIZE * Vec.NORTH;
                oppositeY = problem.StageTop;
            }
            else
            {
                vertical = GRID_SIZE * Vec.SOUTH;
                oppositeY = problem.StageBottom;
            }

            Vec horizontal;
            if (stageStartingCorner.X == problem.StageLeft)
            {
                horizontal = GRID_SIZE * Vec.EAST;
                oppositeX = problem.StageRight;
            }
            else
            {
                horizontal = GRID_SIZE * Vec.WEST;
                oppositeX = problem.StageLeft;
            }

            Point startingCorner = stageStartingCorner + horizontal + vertical;
            edgePoints.Add(startingCorner);
            AddPointsAlongEdge(edgePoints, problem, startingCorner, horizontal);
            Point horizontalLast = edgePoints.Last();
            AddPointsAlongEdge(edgePoints, problem, startingCorner, vertical);
            Point verticalLast = edgePoints.Last();

            Point oppositeCorner = new Point(oppositeX, oppositeY) - horizontal - vertical;
            edgePoints.Add(oppositeCorner);
            AddPointsAlongEdge(edgePoints, problem, oppositeCorner, -horizontal);
            if (verticalLast.DistSq(edgePoints.Last()) <= 100.0000001)
            {
                edgePoints.RemoveAt(edgePoints.Count - 1);
            }

            AddPointsAlongEdge(edgePoints, problem, oppositeCorner, -vertical);
            if (horizontalLast.DistSq(edgePoints.Last()) <= 100.0000001)
            {
                edgePoints.RemoveAt(edgePoints.Count - 1);
            }

            return edgePoints;
        }

        // startingCorner already accounts for the margin
        private static void AddPointsAlongEdge(List<Point> edgePoints, ProblemSpec problem, Point startingCorner, Vec step)
        {
            for (Point p = startingCorner + step; IsPointOnStage(p, problem); p += step)
            {
                edgePoints.Add(p);
            }
        }

        private static bool IsPointOnStage(Point p, ProblemSpec problem)
        {
            return p.X >= problem.StageLeft + GRID_SIZE && 
                p.X <= problem.StageRight - GRID_SIZE && 
                p.Y >= problem.StageBottom + GRID_SIZE && 
                p.Y <= problem.StageTop - GRID_SIZE;
        }
    }
}
