using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class LetsGetCrackin
    {
        public static Solution Solve(ProblemSpec problem, SharedSettings settings, UIAdapter ui)
        {
            Solution solution = new Solution(problem);

            List<Point> allocatedLocations = new List<Point>();
            foreach (Side s in problem.Stage.Sides)
            {
                List<Attendee> attendeesToWatch = Utils.SelectAttendees(problem, s, ComputeScore);
                List<Point> sideLocations = AllocateLocations(attendeesToWatch, s);
                AddNonColliding(allocatedLocations, sideLocations);
            }

            // Is the size too small?
            if (allocatedLocations.Count > solution.Problem.Musicians.Count) 
            {
                return solution;
            }

            for (int i = 0; i < allocatedLocations.Count; i++)
            {
                solution.SetPlacement(solution.Problem.Musicians[i], allocatedLocations[i]);
            }

            Rect innerDumpSpace = problem.Stage.Shrink(Utils.GRID_SIZE * 3);
            List<Point> dumpStuff = Utils.GridPoints(innerDumpSpace);
            for (int i = allocatedLocations.Count; i < problem.Musicians.Count; i++)
            {
                solution.SetPlacement(solution.Problem.Musicians[i], dumpStuff[i]);
            }
            
            // Foreach edge
            //   Figure out which people to target, place the cracks and crack watchers
            //   Fill up the rest of the space on that line
            // Toss people in the middle somwehere (close to edges, ideally)


            // Anneal?  Be happy?



            ui.Render(solution);
            return AnnealingSolver.Solve(solution, AnnealingSolver.ComputeCost, 45000, 1000000);
        }

        private static void AddNonColliding(List<Point> allocatedLocations, List<Point> sideLocations)
        {
            List<Point> nonColliding = new List<Point>();
            foreach (Point newPoint in sideLocations)
            {
                bool collides = false;
                foreach (Point oldPoint in allocatedLocations)
                {
                    if (Utils.MusiciansCollide(newPoint, oldPoint))
                    {
                        collides = true;
                        break;
                    }
                }

                if (!collides)
                {
                    nonColliding.Add(newPoint);
                }
            }

            allocatedLocations.AddRange(nonColliding);
        }

        private static double ComputeScore(double wastedSpace, Side stageSide, Attendee crack)
        {
            double score = -wastedSpace * 10000000;
            if (crack != null)
            {
                double distance = stageSide.OutwardComponent(crack.Location);
                score += crack.Tastes.Average() / (distance * distance);
            }

            return score;
        }

        private static List<Point> AllocateLocations(List<Attendee> attendees, Side side)
        {
            List<Point> allocatedLocations = new List<Point>();
            Side musicianSide = side.Shrink(Utils.GRID_SIZE / 2).Translate(-side.Outward * Utils.GRID_SIZE);
            var crackPoints = new List<Point>() { musicianSide.Left };
            crackPoints.AddRange(attendees.Select(x => musicianSide.Project(x.Location)));
            crackPoints.Add(musicianSide.Right);

            for (int i = 0; i < crackPoints.Count - 1; i++)
            {
                Point spanStart = crackPoints[i];
                Point spanEnd = crackPoints[i + 1];
                Vec step = side.Along * Utils.GRID_SIZE;
                int musicianCount = ((int)spanStart.Manhattan(spanEnd)) / 10;
                Point currentPoint = spanStart + side.Along * Utils.GRID_SIZE / 2;
                for (int j = 0; j < musicianCount; j++)
                {
                    allocatedLocations.Add(currentPoint);
                    currentPoint += step;
                }

                if (i != 0)
                {
                    Point watcherLocation = spanStart - side.Outward * (Utils.GRID_SIZE * Math.Sqrt(3) / 2);
                    if (spanStart != spanEnd)
                    {
                        allocatedLocations.Add(watcherLocation);
                    }
                }
            }

            return allocatedLocations;
        }
    }
}
