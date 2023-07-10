using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public class FixedPointSolution
    {
        public List<Point> SlotLocations { get; }
        public List<int> Slots { get; }
        private StaticBlockingComputer BlockingComputerTron;
        private ProblemSpec Problem;

        private List<double> Qs;
        private List<HashSet<int>> InstrumentToUsedSlots;
        private List<double> SlotScores;

        private double ScoreCache;

        public FixedPointSolution(ProblemSpec problem, List<Point> slotLocations)
            : this(problem, slotLocations, new StaticBlockingComputer(slotLocations, problem.Attendees, problem.Pillars)) { }

        private FixedPointSolution(ProblemSpec problem, List<Point> slotLocations, StaticBlockingComputer computer)
        {
            SlotLocations = slotLocations;
            BlockingComputerTron = computer;
            Problem = problem;
            SlotScores = new();
            Qs = new List<double>();
            for (int i = 0; i < slotLocations.Count; i++)
            {
                Qs.Add(double.NaN);
                SlotScores.Add(0);
            }

            Slots = new List<int>();
            InstrumentToUsedSlots = new List<HashSet<int>>();
            for (int i = 0; i < problem.Musicians.Max(x => x.Instrument) + 1; i++)
            {
                InstrumentToUsedSlots.Add(new HashSet<int>());
            }

            foreach (var _ in slotLocations)
            {
                Slots.Add(-1);
            }
        }

        public void SetInstrument(int slot, int instrument)
        {
            if (Slots[slot] != -1)
            {
                throw new Exception("Slot already filled");
            }

            if (instrument == -1)
            {
                return;
            }

            InstrumentToUsedSlots[instrument].Add(slot);
            Qs[slot] = UpdateQs(slot, instrument, 1);
            Slots[slot] = instrument;

            double scoreDiff = ComputeScoreForSlot(slot);
            UpdateScoreCache(slot, scoreDiff);
        }

        public int RemoveInstrument(int slot)
        {
            if (Slots[slot] == -1)
            {
                return -1;
            }


            int instrument = Slots[slot];
            InstrumentToUsedSlots[instrument].Remove(slot);
            UpdateScoreCache(slot, -SlotScores[slot]);
            SlotScores[slot] = 0;

            Slots[slot] = -1;
            Qs[slot] = double.NaN;
            UpdateQs(slot, instrument, -1);
            return instrument;
        }

        public long Swap(int s0, int s1)
        {
            if (Slots[s0] == Slots[s1])
            {
                throw new Exception("Bite me");
            }

            int newS0 = RemoveInstrument(s1);
            int newS1 = RemoveInstrument(s0);
            SetInstrument(s0, newS0);
            SetInstrument(s1, newS1);
            return (long)ScoreCache;
        }

        public long GetScore()
        {
            return (long)ScoreCache;
        }

        public void AssertValidity()
        {
            FixedPointSolution fresh = new FixedPointSolution(Problem, SlotLocations, BlockingComputerTron);
            for (int slot = 0; slot < Slots.Count; slot++)
            {
                fresh.SetInstrument(slot, Slots[slot]);
            }

            for (int slot = 0; slot < Slots.Count; slot++)
            {
                double q = Qs[slot];
                double freshQ = fresh.Qs[slot];
                if (!AlmostEqual(q, freshQ, 1E-12))
                {
                    throw new Exception("Qs don't match");
                }
            }
        }

        public static Solution MatchingToSolution(ProblemSpec problem, List<Point> fixedPoints, List<int> matches)
        {
            Solution solution = new Solution(problem);
            List<Musician> draftees = problem.Musicians.ToList();
            for (int slotNumber = 0; slotNumber < matches.Count; slotNumber++)
            {
                if (matches[slotNumber] == -1) continue;
                for (int draftIndex = 0; draftIndex < draftees.Count; draftIndex++)
                {
                    if (draftees[draftIndex].Instrument == matches[slotNumber])
                    {
                        solution.SetPlacement(draftees[draftIndex], fixedPoints[slotNumber]);
                        draftees.RemoveAt(draftIndex);
                        break;
                    }
                }
            }

            return solution;
        }

        private static bool AlmostEqual(double a, double b, double epsilon)
        {
            if (Double.IsNaN(a) || Double.IsNaN(b))
            {
                return Double.IsNaN(a) && Double.IsNaN(b);
            }
            else
            {
                return Math.Abs(a - b) < epsilon;
            }
        }

        private void UpdateScoreCache(int slot, double scoreDiff)
        {
            if (Problem.UsePlayingTogetherScoring)
            {
                ScoreCache += Qs[slot] * scoreDiff;
            }
            else
            {
                ScoreCache += scoreDiff;
            }
        }

        private double UpdateQs(int changingSlot, int instrument, int modifier)
        {
            if (!Problem.UsePlayingTogetherScoring)
            {
                return double.NaN;
            }

            double changingSlotQ = modifier;
            foreach (int otherSlot in InstrumentToUsedSlots[instrument])
            {
                if (changingSlot == otherSlot)
                {
                    continue;
                }

                double update = modifier / SlotLocations[changingSlot].Dist(SlotLocations[otherSlot]);

                // Update the other slot's Q, which means the score cache changes also
                double otherScore = SlotScores[otherSlot];
                UpdateScoreCache(otherSlot, -otherScore);
                Qs[otherSlot] += update;
                UpdateScoreCache(otherSlot, otherScore);

                changingSlotQ += update;
            }

            return changingSlotQ;
        }

        private double ComputeScoreForSlot(int slot)
        {
            double score = 0;
            foreach (var attendee in BlockingComputerTron.GetVisibleAttendees(slot))
            {
                score += PairScore(Slots[slot], attendee, SlotLocations[slot]);
            }

            SlotScores[slot] = score;
            return score;
        }

        private static double PairScore(int instrument, Attendee attendee, Point location)
        {
            return 1000000 * attendee.Tastes[instrument] / attendee.Location.DistSq(location);
        }
    }
}
