using System;
using System.Linq;
using Dalamud.Logging;
using RaidBuffTracker.Tracker.Library;
using RaidBuffTracker.Utils;
using StatusInstance = Dalamud.Game.ClientState.Statuses.Status;

namespace RaidBuffTracker.Tracker.Track
{
    public sealed class ActionTrack
    {
        public readonly ActionTrackSource source;
        public readonly ActionLibraryRecord record;

        public double lastInvoked;
        public double lastActivated;

        public bool IsActive => DurationRemaining > 0;
        public bool IsReady => CooldownRemaining < 0;
        public double DurationRemaining => lastActivated + record.duration - DateTime.Now.TimestampSeconds();
        public double CooldownRemaining => lastInvoked + record.cooldown - DateTime.Now.TimestampSeconds();

        public ActionTrack(ActionTrackSource source, ActionLibraryRecord record)
        {
            this.source = source;
            this.record = record;
        }

        public void Reset()
        {
            PluginLog.Debug(string.Format("Reseting track {0}", this), Array.Empty<object>());
            lastInvoked = 0.0;
            lastActivated = 0.0;
        }

        public void UpdateWithInvocation(uint actionId)
        {
            if (record.cooldownActionIds.Contains(actionId))
            {
                PluginLog.Debug(string.Format("Updating track {0} with invocation of {1}", this, actionId),
                    Array.Empty<object>());
                lastInvoked = DateTime.Now.TimestampSeconds() + 0.55;
            }

            if (record.effectActionIds.Contains(actionId))
            {
                PluginLog.Debug(string.Format("Updating track {0} with effect of {1}", this, actionId),
                    Array.Empty<object>());
                lastActivated = DateTime.Now.TimestampSeconds() + 0.55;
            }
        }

        public override string ToString()
        {
            return $"ActionTrack ({source.index}/{source.name} - {record.name})";
        }
    }
}