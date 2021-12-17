using System;
using RaidBuffTracker.Tracker.Library;
using RaidBuffTracker.Utils;
using StatusInstance = Dalamud.Game.ClientState.Statuses.Status;

namespace RaidBuffTracker.Tracker.Track
{
    public sealed class StatusTrack
    {
        public int Identifier => CalculateIdentifier(source.objectId, record.statusId);

        public readonly StatusTrackSource   source;
        public readonly StatusLibraryRecord record;

        public double lastInvoked;

        public bool   IsActive          => lastInvoked + record.duration > DateTime.Now.TimestampSeconds();
        public bool   IsReady           => CooldownRemaining < 0;
        public double DurationRemaining => lastInvoked + record.duration - DateTime.Now.TimestampSeconds();
        public double CooldownRemaining => lastInvoked + record.cooldown - DateTime.Now.TimestampSeconds();

        public StatusTrack(StatusTrackSource source, StatusLibraryRecord @record)
        {
            this.source = source;
            this.record = record;
        }

        public static int CalculateIdentifier(uint objectId, uint statusId)
        {
            var hash = 27;
            hash = (13 * hash) + objectId.GetHashCode();
            hash = (13 * hash) + statusId.GetHashCode();
            return hash;
        }

        public void Update(double remainingTime)
        {
            var durationRemaining = record.duration - remainingTime;
            lastInvoked = DateTime.Now.TimestampSeconds() - durationRemaining;
        }
    }
}