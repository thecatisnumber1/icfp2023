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
            this.width = (int)Math.Ceiling(solution.Problem.RoomWidth / CELL_SIZE);
            this.height = (int)Math.Ceiling(solution.Problem.RoomHeight / CELL_SIZE);
            this.cells = new Cell[width, height];

            // Add pillars to cells. This just adds their bounding square, so some of the
            // cells actually have no chance of intersecting with the pillar.
            // We can optimize that later if it becomes an issue.
            foreach (var pillar in solution.Problem.Pillars)
            {
                int r = (int)Math.Ceiling(pillar.Radius / CELL_SIZE);
                int d = (2 * r) + 1;
                int px = (int)(pillar.Center.X / CELL_SIZE) - r;
                int py = (int)(pillar.Center.Y / CELL_SIZE) - r;

                for (int i = 0; i <= d; i++)
                {
                    for (int j = 0; j <= d; j++)
                    {
                        int cx = px + i;
                        int cy = py + j;

                        if (cx >= 0 && cx < width && cy >= 0 && cy < height) {
                            if (cells[cx, cy] == null)
                            {
                                cells[cx, cy] = new();
                            }

                            cells[cx, cy].Pillars.Add(pillar);
                        }
                    }
                }
            }
        }

        public void OnPlacementChanged(Point newLoc, Point oldLoc)
        {
            if (oldLoc != Point.ORIGIN && oldLoc != Point.INVALID)
            {
                var (oldX, oldY) = CellFor(oldLoc);

                if (cells[oldX, oldY] != null)
                {
                    cells[oldX, oldY].Musicians.Remove(oldLoc);
                }
            }

            if (newLoc != Point.ORIGIN && newLoc != Point.INVALID)
            {
                var (newX, newY) = CellFor(newLoc);

                if (cells[newX, newY] == null)
                {
                    cells[newX, newY] = new();
                }

                cells[newX, newY].Musicians.Add(newLoc);
            }
        }

        private class Cell
        {
            public List<Point> Musicians = new();
            public List<Pillar> Pillars = new();
        }

        private (int, int) CellFor(Point p)
        {
            // Round to nearest cell
            int x = (int)Math.Floor((p.X + (CELL_SIZE / 2)) / CELL_SIZE);
            int y = (int)Math.Floor((p.Y + (CELL_SIZE / 2)) / CELL_SIZE);
            return (x, y);
        }

        public bool IsMusicianBlocked(Musician musician, Attendee attendee)
        {
            Point musicianLoc = solution.GetPlacement(musician);
            HashSet<Cell> visited = new();

            foreach (var cell in OccludingCells(musician, attendee))
            {
                if (cell == null || visited.Contains(cell))
                {
                    continue;
                }

                visited.Add(cell);

                foreach (var blockingMusicianLoc in cell.Musicians)
                {
                    if (musicianLoc != blockingMusicianLoc &&
                        solution.IsMusicianBlocked(attendee.Location, musicianLoc, blockingMusicianLoc))
                    {
                        return true;
                    }
                }

                foreach (var pillar in cell.Pillars)
                {
                    if (solution.IsMusicianBlocked(attendee.Location, musician, pillar))
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
            var (x0, y0) = CellFor(solution.GetPlacement(musician));
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

                if (x0 == x1 && y0 == y1) {
                    break;
                }

                int e2 = 2 * error;

                if (e2 >= dy)
                {
                    if (x0 == x1)
                    {
                        break;
                    }
                    error += dy;
                    x0 += sx;
                }

                if (e2 <= dx)
                {
                    if (y0 == y1)
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
