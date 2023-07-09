using System;
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
        private List<long> SlotScores;

        private long ScoreCache;

        public FixedPointSolution(ProblemSpec problem, List<Point> slotLocations)
            : this(problem, slotLocations, new StaticBlockingComputer(slotLocations, problem.Attendees, problem.Pillars)) { }
        
        private FixedPointSolution(ProblemSpec problem, List<Point> slotLocations, StaticBlockingComputer computer)
        {
            SlotLocations = slotLocations;
            BlockingComputerTron = computer;
            Problem = problem;
            SlotScores = new List<long>();
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
            ComputeScoreForSlot(slot);
        }

        public int RemoveInstrument(int slot)
        {
            if (Slots[slot] == -1)
            {
                return -1;
            }


            int instrument = Slots[slot];
            InstrumentToUsedSlots[instrument].Remove(slot);
            ScoreCache -= SlotScores[slot];
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
            return ScoreCache;
        }

        public long GetScore()
        {
            return ScoreCache;
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


        private double UpdateQs(int changingSlot, int instrument, int modifier)
        {
            double changingSlotQ = modifier;
            foreach (int otherSlot in InstrumentToUsedSlots[instrument])
            {
                if (changingSlot == otherSlot)
                {
                    continue;
                }

                double update = modifier / SlotLocations[changingSlot].Dist(SlotLocations[otherSlot]);
                SetQ(otherSlot, Qs[otherSlot] + update);
                changingSlotQ += update;
            }

            return changingSlotQ;
        }

        private void ComputeScoreForSlot(int slot)
        {
            long score = 0;
            foreach (var attendee in BlockingComputerTron.GetVisibleAttendees(slot))
            {
                score += Problem.PairScore(Slots[slot], attendee.Index, SlotLocations[slot], Qs[slot]);
            }

            SlotScores[slot] = score;
            ScoreCache += score;
        }

        private void SetQ(int slot, double newQ)
        {
            long delta = 0;
            double oldQ = Qs[slot];
            Qs[slot] = newQ;
            foreach (Attendee a in BlockingComputerTron.GetVisibleAttendees(slot))
            {
                delta -= Problem.PairScore(Slots[slot], a.Index, SlotLocations[slot], oldQ);
                delta += Problem.PairScore(Slots[slot], a.Index, SlotLocations[slot], newQ);
            }

            ScoreCache += delta;
            SlotScores[slot] += delta;
        }
    }
}
