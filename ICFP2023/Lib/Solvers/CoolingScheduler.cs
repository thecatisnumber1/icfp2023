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

        public long Iterations;
        public double PrevTempLog;
        public Stopwatch Watch;
        private double lastAcceptance;

        public CoolingScheduler(long targetMs, double initialTemp, double finalTemp)
        {
            TargetMs = targetMs;
            PrevTempLog = Math.Log(initialTemp);
            FinalTempLog = Math.Log(finalTemp);
        }

        public double Temperature => Math.Exp(TempLog);
        public double TempLog => (Watch != null && Watch.ElapsedMilliseconds >= TargetMs ? FinalTempLog : PrevTempLog);

        public void AdvanceTemperature(double acceptance=0.0)
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

            lastAcceptance = acceptance;
        }

        public long RemainingMs()
        {
            if (Watch == null)
            {
                Watch = Stopwatch.StartNew();
            }
            return TargetMs - (long)Watch.ElapsedMilliseconds;
        }

        public bool ICE_COLD()
        {
            if (Watch == null)
            {
                return false;
            }

            return lastAcceptance == 0 && Watch.ElapsedMilliseconds >= TargetMs;
        }
    }
}