using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;

namespace SyncHameleon
{
    public static class StopwatchHelper
    {
        private static Dictionary<string, Stopwatch> stopwatches = new Dictionary<string, Stopwatch>();
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Start(string stopwatchName)
        {
            logger.Trace("-> "+ stopwatchName);
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatches[stopwatchName] = stopwatch;
        }

        public static void Stop(string stopwatchName)
        {
            Stopwatch stopwatch = stopwatches[stopwatchName];
            stopwatch.Stop();
            //stopwatches.Remove(stopwatchName);
            long milliseconds = stopwatch.ElapsedMilliseconds;
            TimeSpan ts = stopwatch.Elapsed;

            // Format and display the TimeSpan value.
            GlobalDiagnosticsContext.Set("TimeSpan", milliseconds);
            logger.Info("<- "+stopwatchName+":{0}",
            String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10));
            GlobalDiagnosticsContext.Set("TimeSpan", null);
        }
    }
}
