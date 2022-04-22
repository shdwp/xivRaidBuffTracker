using RaidBuffTracker.Tracker.Track;
using Dalamud.Logging;
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

        public bool IsApplicableToSource(ActionTrackSource? source)
        {
            if (source == null) return false;
            //PluginLog.Warning("job affin {x}, minlvl {y}, source.lvl {z}", jobAffinity, minLvl, source.lvl);
            return jobAffinity.Contains(source.Value.jobAbbr) && source.Value.lvl >= minLvl;
            // return source.lvl >= minLvl && jobAffinity.Contains(source.jobAbbr);
        }
    }
}