using System;
using Dalamud.Game.ClientState.Statuses;

namespace RaidBuffTracker.Tracker.Source
{
    public interface IStatusTrackerSource
    {
        public event Action<Status> StatusDetected;
    }
}