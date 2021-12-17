using System.Collections.Generic;
using RaidBuffTracker.Tracker.Track;

namespace RaidBuffTracker.Tracker
{
    public interface IStatusTracker
    {
        IEnumerable<StatusTrack> EnumerateTracks();
    }
}