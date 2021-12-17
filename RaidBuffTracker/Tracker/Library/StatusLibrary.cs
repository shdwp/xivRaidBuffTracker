using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Data;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Party;
using Dalamud.Logging;
using Lumina.Data;
using Lumina.Excel;
using RaidBuffTracker.Tracker.Track;
using Action = Lumina.Excel.GeneratedSheets.Action;
using Status = Dalamud.Game.ClientState.Statuses.Status;

namespace RaidBuffTracker.Tracker.Library
{
    public sealed class StatusLibrary
    {
        private DataManager _dataManager;

        private ExcelSheet<Action>                              _actionSheet;
        private ExcelSheet<Lumina.Excel.GeneratedSheets.Status> _statusSheet;

        private readonly Dictionary<uint, StatusLibraryRecord> _db = new();

        public StatusLibrary(DataManager dataManager)
        {
            _dataManager = dataManager;
            _actionSheet = dataManager.GameData.GetExcelSheet<Action>(Language.English);
            _statusSheet = dataManager.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Status>(Language.English);
            if (_actionSheet == null || _statusSheet == null)
            {
                // @TODO
                throw new InvalidOperationException("");
            }

            RegisterAll();
        }

        public bool ShouldTrack(Status status)
        {
            return _db.ContainsKey(status.StatusId);
        }

        public StatusLibraryRecord FindRecord(uint id)
        {
            return _db.GetValueOrDefault(id);
        }

        public IReadOnlyCollection<StatusLibraryRecord> AllRecords()
        {
            return _db.Values;
        }

        public StatusTrack InstantiateTrack(Status status)
        {
            if (!(status.SourceObject is PlayerCharacter character))
            {
                throw new ArgumentException("Invalid status passed - source not player!");
            }

            if (!_db.TryGetValue(status.StatusId, out var record))
            {
                throw new ArgumentException("Invalid status passed - not found in db!");
            }

            return new StatusTrack(character.ToStatusTrackSource(), record);
        }

        public StatusTrack InstantiateTrack(PlayerCharacter playerCharacter, StatusLibraryRecord @record)
        {
            return new StatusTrack(playerCharacter.ToStatusTrackSource(), record);
        }

        public IEnumerable<StatusTrack> InstantiatePartyTracks(IEnumerable<PartyMember> members)
        {
            foreach (var record in _db.Values)
            {
                foreach (var member in members)
                {
                    if (member.ClassJob.Id == record.jobId && member.Level >= record.minLvl)
                    {
                        yield return new StatusTrack(member.ToStatusTrackSource(), record);
                    }
                }
            }
        }

        public IEnumerable<StatusTrack> InstantiatePlayerTracks(PlayerCharacter playerCharacter)
        {
            foreach (var record in _db.Values)
            {
                if (playerCharacter.ClassJob.Id == record.jobId && playerCharacter.Level >= record.minLvl)
                {
                    yield return new StatusTrack(playerCharacter.ToStatusTrackSource(), record);
                }
            }
        }

        private void RegisterAll()
        {

        }

        private void Register(uint actionId, uint statusId)
        {
            var action = _actionSheet.GetRow(actionId);
            if (action == null)
            {
                PluginLog.Warning($"Failed to find Action by id {actionId}");
                return;
            }

            var status = _statusSheet.FirstOrDefault(s => s.Name.ToString() == actionName);
            if (status == null || status.RowId == 0)
            {
                PluginLog.Warning($"Failed to get associated status of action by name {actionName}");
                return;
            }
            PluginLog.Warning($"job {action.ClassJob.Row} lvl {action.ClassJobLevel}");
            Register(new StatusLibraryRecord
            {
                actionId = action.RowId,
                statusId = status.RowId,
                iconId = action.Icon,
                jobId = action.ClassJob.Row,
                minLvl = action.ClassJobLevel,
                duration = 60.0,
                cooldown = 30.0,
            });
        }

        private void Register(StatusLibraryRecord @record)
        {
            if (_db.ContainsKey(@record.statusId))
            {
                throw new InvalidOperationException($"Status with {record.statusId} already registered!");
            }

            _db[@record.statusId] = record;
        }
    }
 }