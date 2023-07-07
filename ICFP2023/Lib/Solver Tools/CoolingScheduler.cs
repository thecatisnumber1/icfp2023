using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ICFP2023
{
    public class CoolingScheduler
    {
        public readonly long TargetMs;
        public readonly double FinalTempLog;

        private const long FakeMillis = 50;
        private const long FakeIterations = 500;

        private long Iterations;
        private double PrevTempLog;
        private Stopwatch Watch;

        public CoolingScheduler(long targetMs, double initialTemp, double finalTemp)
        {
            TargetMs = targetMs;
            PrevTempLog = Math.Log(initialTemp);
            FinalTempLog = Math.Log(finalTemp);
        }

        public double Temperature => Math.Exp(PrevTempLog);

        public void AdvanceTemperature()
        {
            // First iteration
            if (Watch == null)
            {
                Watch = Stopwatch.StartNew();
            }

            Iterations++;
            double iterationsPerMs = ((double)(Iterations + FakeIterations)) / (Watch.ElapsedMilliseconds + FakeMillis);
            long remainingIterations = (long)((TargetMs - Watch.ElapsedMilliseconds) * iterationsPerMs);
            double delta = (FinalTempLog - PrevTempLog) / remainingIterations;
            PrevTempLog += delta;
        }

        public long RemainingMs()
        {
            return TargetMs - (long)Watch.ElapsedMilliseconds;
        }

        public bool ICE_COLD()
        {
            if (Watch == null)
            {
                return false;
            }

            return Watch.ElapsedMilliseconds >= TargetMs;
        }
    }
}