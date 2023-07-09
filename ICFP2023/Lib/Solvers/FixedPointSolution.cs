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

        private long ScoreCache;
        
        public FixedPointSolution(ProblemSpec problem, List<Point> slotLocations)
        {
            SlotLocations = slotLocations;
            BlockingComputerTron = new StaticBlockingComputer(slotLocations, problem.Attendees, problem.Pillars);
            Problem = problem;
            Slots = new List<int>();
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

            Slots[slot] = instrument;
            foreach (var attendee in BlockingComputerTron.GetVisibleAttendees(slot))
            {
                ScoreCache += Problem.PairScore(instrument, attendee.Index, SlotLocations[slot], 1);
            }
        }

        public int RemoveInstrument(int slot)
        {
            if (Slots[slot] == -1)
            {
                return -1;
            }

            int instrument = Slots[slot];
            foreach (var attendee in BlockingComputerTron.GetVisibleAttendees(slot))
            {
                ScoreCache -= Problem.PairScore(instrument, attendee.Index, SlotLocations[slot], 1);
            }

            Slots[slot] = -1;
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
    }
}
