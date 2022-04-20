using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Party;

namespace RaidBuffTracker.Tracker.Track
{
    public struct ActionTrackSource
    {
        public uint   objectId;
        public string name;
        public uint   index;

        public uint   jobId;
        public string jobAbbr;
        public uint   lvl;
    }

    public static class SourceExtensions
    {
        public static ActionTrackSource ToActionTrackSource(this PartyMember member, uint index)
        {
            if (member.ClassJob.GameData == null)
            {
                return new ActionTrackSource
                {
                    index = index,
                    objectId = member.ObjectId,
                    name = member.Name.ToString(),
                    jobId = member.ClassJob.Id,
                    jobAbbr = "",
                    lvl = member.Level,
                };
            }
            return new ActionTrackSource
            {
                index = index,
                objectId = member.ObjectId,
                name = member.Name.ToString(),
                jobId = member.ClassJob.Id,
                jobAbbr = member.ClassJob.GameData.Abbreviation,
                lvl = member.Level,
            };
        }

        public static ActionTrackSource ToActionTrackSource(this PlayerCharacter character, uint index)
        {
            if (character.ClassJob.GameData == null)
            {
                return new ActionTrackSource
                {
                    index = index,
                    objectId = character.ObjectId,
                    name = character.Name.ToString(),
                    jobId = character.ClassJob.Id,
                    jobAbbr = "",
                    lvl = character.Level,
                };
            }
            return new ActionTrackSource
            {
                index = index,
                objectId = character.ObjectId,
                name = character.Name.ToString(),
                jobId = character.ClassJob.Id,
                jobAbbr = character.ClassJob.GameData.Abbreviation,
                lvl = character.Level,
            };
        }
    }
}