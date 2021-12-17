using System;

namespace RaidBuffTracker.Utils
{
    public static class Timestamp
    {
        public static DateTime timestampStart = DateTime.Today;

        public static double TimestampSeconds(this DateTime dt) => ((dt.ToUniversalTime() - timestampStart).TotalSeconds);
    }
}