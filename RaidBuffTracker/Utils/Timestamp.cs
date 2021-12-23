using System;

namespace RaidBuffTracker.Utils
{
    public static class Timestamp
    {
        public static DateTime timestampStart = DateTime.Today - TimeSpan.FromDays(1);

        public static double TimestampSeconds(this DateTime dt) => (dt - timestampStart).TotalSeconds;
    }
}