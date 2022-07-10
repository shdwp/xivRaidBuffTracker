using System.Collections.Generic;
using RaidBuffTracker.Tracker.Track;

namespace RaidBuffTracker.Tracker
{
    public static class ActionTracker
    {
        public static string regular = "regular";
        public static string testingDisplay = "testingDisplay";
    }

    public interface IActionTracker
    {
        IEnumerable<ActionTrack> EnumerateTracks();
        bool AnyTracks();
    }
}