using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class LetsGetCrackin
    {
        public static Solution Solve(ProblemSpec problem, SharedSettings settings, UIAdapter ui)
        {
            Solution solution = new Solution(problem);
            List<Point> allocatedLocations = PlacePointsAlongEdgesAKAGetCrackin(problem);

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

            ui.Render(solution);
            solution = AnnealingSolver.Solve(solution, AnnealingSolver.ComputeCost, 20000, 200000);
            return solution;
        }

        public static Solution FixedPointAnnealSolve(ProblemSpec problem, SharedSettings settings, UIAdapter ui)
        {
            List<Point> fixedPoints = PlacePointsAlongEdgesAKAGetCrackin(problem);
            List<Point> initialFixedPoints = fixedPoints.ToList();
            Rect innerRect = problem.Stage.Shrink(Utils.GRID_SIZE * 2);
            List<Point> innerPoints = Utils.GridPoints(innerRect);
            fixedPoints.AddRange(innerPoints);
            CleanInvalidPoints(initialFixedPoints, fixedPoints);
            if (fixedPoints.Count < problem.Musicians.Count)
            {
                return new Solution(problem);
            }

            List<int> matches = FixedPointMatcher.FindMatching(problem, fixedPoints, ui, 90000, 200000);
            Solution solution = FixedPointSolution.MatchingToSolution(problem, fixedPoints, matches);

            Console.WriteLine($"{solution.InitializeScore()}");
            ui.Render(solution);
            ColorUtil.ColorInstruments(ui, problem);

            return solution;
        }

        private static void CleanInvalidPoints(List<Point> edge, List<Point> all)
        {
            List<Point> accepted = new List<Point>();
            foreach (Point p in all)
            {
                if (edge.Contains(p))
                {
                    accepted.Add(p);
                }

                bool collides = false;
                foreach (Point e in edge)
                {
                    if (Utils.MusiciansCollide(p, e))
                    {
                        collides = true;
                        break;
                    }
                }

                if (!collides)
                {
                    accepted.Add(p);
                }
            }

            all.Clear();
            all.AddRange(accepted);
        }

        private static List<Point> PlacePointsAlongEdgesAKAGetCrackin(ProblemSpec problem)
        {
            List<Point> allocatedLocations = new List<Point>();
            foreach (Side s in problem.Stage.Sides)
            {
                List<Attendee> attendeesToWatch = Utils.SelectAttendees(problem, s, ComputeScore);
                List<Point> sideLocations = AllocateLocations(attendeesToWatch, s);
                AddNonColliding(allocatedLocations, sideLocations);
            }

            return allocatedLocations;
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
            double score = -wastedSpace * 1000;
            if (crack != null)
            {
                double distance = stageSide.OutwardComponent(crack.Location);
                score += crack.Tastes.Max() / (distance * distance);
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
                double spanLength = spanStart.Manhattan(spanEnd);
                int musicianCount = ((int)spanLength) / 10;
                double remainingSpace = spanLength - musicianCount * Utils.GRID_SIZE;
                Point currentPoint = spanStart + side.Along * (Utils.GRID_SIZE + remainingSpace) / 2;

                if (i != 0)
                {
                    Point previousPoint = allocatedLocations[^1];
                    Point midPoint = currentPoint.Mid(previousPoint);
                    double spacing = currentPoint.Manhattan(previousPoint) / 2;
                    double distanceFromEdge = Math.Sqrt(100 - spacing * spacing) + 0.000001;
                    Point watcherLocation = midPoint - side.Outward * distanceFromEdge;
                    allocatedLocations.Add(watcherLocation);
                }

                Vec step = side.Along * Utils.GRID_SIZE;
                for (int j = 0; j < musicianCount; j++)
                {
                    allocatedLocations.Add(currentPoint);
                    currentPoint += step;
                }
                
            }

            return allocatedLocations;
        }
    }
}
