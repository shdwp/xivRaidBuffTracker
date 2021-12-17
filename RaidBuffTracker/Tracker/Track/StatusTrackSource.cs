using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Party;

namespace RaidBuffTracker.Tracker.Track
{
    public sealed class StatusTrackSource
    {
        public uint   objectId;
        public string name;

        public uint jobId;
        public uint lvl;
    }

    public static class SourceExtensions
    {
        public static StatusTrackSource ToStatusTrackSource(this PartyMember member)
        {
            return new StatusTrackSource
            {
                objectId = member.ObjectId,
                name = member.Name.ToString(),
                jobId = member.ClassJob.Id,
                lvl = member.Level,
            };
        }

        public static StatusTrackSource ToStatusTrackSource(this PlayerCharacter character)
        {
            return new StatusTrackSource
            {
                objectId = character.ObjectId,
                name = character.Name.ToString(),
                jobId = character.ClassJob.Id,
                lvl = character.Level,
            };
        }
    }
}