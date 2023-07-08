using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class OcclusionFinder
    {
        private const int CELL_SIZE = 10;

        private readonly Solution solution;
        private readonly int width;
        private readonly int height;
        private readonly Cell[,] cells;

        public OcclusionFinder(Solution solution)
        {
            this.solution = solution;
            this.width = (int)Math.Ceiling(solution.Problem.StageWidth / CELL_SIZE);
            this.height = (int)Math.Ceiling(solution.Problem.StageHeight / CELL_SIZE);
            this.cells = new Cell[width, height];
        }

        private Point GetPlacement(Musician musician)
        {
            return solution.GetPlacement(musician);
        }

        public void OnPlacementChanged(Musician musician, Point oldLoc)
        {
            if (oldLoc != Point.ORIGIN)
            {
                var (oldX, oldY) = CellFor(oldLoc);

                if (cells[oldX, oldY] != null)
                {
                    cells[oldX, oldY].Musicians.Remove(musician);
                }
            }

            var (newX, newY) = CellFor(GetPlacement(musician));

            if (cells[newX, newY] == null)
            {
                cells[newX, newY] = new();
            }

            cells[newX, newY].Musicians.Add(musician);
        }

        private class Cell
        {
            public List<Musician> Musicians = new();
        }

        private (int, int) CellFor(Point p)
        {
            // Round to nearest cell
            int x = (int)(p.X - solution.Problem.StageBottomLeft.X + (CELL_SIZE / 2)) / CELL_SIZE;
            int y = (int)(p.Y - solution.Problem.StageBottomLeft.Y + (CELL_SIZE / 2)) / CELL_SIZE;
            return (x, y);
        }

        public bool IsMusicianBlocked(Musician musician, Attendee attendee)
        {
            HashSet<Cell> visited = new();

            foreach (var cell in OccludingCells(musician, attendee))
            {
                if (cell == null || visited.Contains(cell))
                {
                    continue;
                }

                visited.Add(cell);

                foreach (var blockingMusician in cell.Musicians)
                {
                    if (musician != blockingMusician &&
                        solution.IsMusicianBlocked(attendee.Location, musician, blockingMusician))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private IEnumerable<Cell> OccludingCells(Musician musician, Attendee attendee)
        {
            // This is the integer form of Bresenham's line algorithm
            var (x0, y0) = CellFor(GetPlacement(musician));
            var (x1, y1) = CellFor(attendee.Location);

            int dx = Math.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;
            int error = dx + dy;

            while (true)
            {
                // Make the line width 3 so we see potential occluders in neighboring cells
                for (int xx = -1; xx <= 1; xx++)
                {
                    for (int yy = -1; yy <= 1; yy++)
                    {
                        if (x0 + xx >= 0 && x0 + xx < width && y0 + yy >= 0 && y0 + yy < height)
                        {
                            yield return cells[x0 + xx, y0 + yy];
                        }
                    }
                }

                // In practice this will never be true because attendees are not on the stage
                if (x0 == x1 && y0 == y1) {
                    break;
                }

                int e2 = 2 * error;

                if (e2 >= dy)
                {
                    if (x0 == width)
                    {
                        break;
                    }
                    error += dy;
                    x0 += sx;
                }

                if (e2 <= dx)
                {
                    if (y0 == height)
                    {
                        break;
                    }
                    error += dx;
                    y0 += sy;
                }
            }
        }
    }
}
