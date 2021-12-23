using RaidBuffTracker.Tracker.Track;

namespace RaidBuffTracker.Tracker.Library
{
    public struct ActionLibraryRecord
    {
        public string         name;
        public ActionCategory category;
        public uint           iconId;
        public string         tooltip;

        public string jobAffinity;
        public uint   minLvl;

        public uint[] cooldownActionIds;
        public uint[] effectActionIds;

        public uint   actionId;
        public double cooldown;
        public double duration;

        public override string ToString()
        {
            return $"LibraryRecord {name}(actionId={actionId})";
        }

        public bool IsApplicableToSource(ActionTrackSource source)
        {
            return jobAffinity.Contains(source.jobAbbr);
            // return source.lvl >= minLvl && jobAffinity.Contains(source.jobAbbr);
        }
    }
}