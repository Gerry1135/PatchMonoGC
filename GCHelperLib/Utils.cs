using System;
using System.Diagnostics;
using System.Text;

namespace GCHelperLib
{
    public class Utils
    {
        private static double ticksPerMilliSec = 0.0;

        private static long startTime = 0;
        private static long endTime = 0;

        private static String timeFormat = "{0:f3}ms";

        public static void StartCollect()
        {
            startTime = Stopwatch.GetTimestamp();
        }

        public static void StopCollect()
        {
            endTime = Stopwatch.GetTimestamp();
        }

        public static String GetLastTime()
        {
            if (ticksPerMilliSec == 0)
                ticksPerMilliSec = (double)Stopwatch.Frequency / 1000.0;
            return String.Format(timeFormat, (double)(endTime - startTime) / ticksPerMilliSec);
        }
    }
}
