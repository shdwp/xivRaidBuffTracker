using System;
using System.Diagnostics;
using Dalamud.Logging;

namespace RaidBuffTracker.Utils
{
    public static class Diag
    {
        public static void Assert(bool condition, string msg = null)
        {
            if (!condition)
            {
#if DEBUG
                throw new InvalidOperationException($"Assert failed: {msg}");
#else
                PluginLog.Fatal($"Assert failed: {msg}");
                PluginLog.Fatal($"TRACE: {Environment.StackTrace}");
#endif
            }
        }

        public static void MeasureTime(string desc, Action cb)
        {
            var sw = new Stopwatch();
            cb();
            PluginLog.Information($"Measured {desc}: {sw.Elapsed.TotalMilliseconds}ms");
        }
    }
}