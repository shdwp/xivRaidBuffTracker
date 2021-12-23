using System;

namespace RaidBuffTracker.Tracker.Source
{
    public interface IActionTrackerSource
    {
        public event Action<uint, uint> ActionInvocationDetected;
    }
}