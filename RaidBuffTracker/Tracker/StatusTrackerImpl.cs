using System;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Logging;
using RaidBuffTracker.Tracker.Library;
using RaidBuffTracker.Tracker.Source;
using RaidBuffTracker.Tracker.Track;

namespace RaidBuffTracker.Tracker
{
    public sealed class StatusTrackerImpl : IStatusTracker, IDisposable
    {
        private readonly StatusLibrary        _statusLibrary;
        private readonly IStatusTrackerSource _source;
        private readonly PartyList            _partyList;
        private readonly Framework            _framework;
        private readonly ClientState          _clientState;

        private int _lastPartyHash = 0;

        private readonly Dictionary<int, StatusTrack> _tracks = new();

        public StatusTrackerImpl(StatusLibrary statusLibrary, IStatusTrackerSource source, PartyList partyList, Framework framework, ClientState clientState)
        {
            _statusLibrary = statusLibrary;
            _source = source;
            _partyList = partyList;
            _framework = framework;
            _clientState = clientState;

            _framework.Update += OnFrameworkUpdate;
            _source.StatusDetected += OnStatusDetected;
        }

        public void Dispose()
        {
            _source.StatusDetected -= OnStatusDetected;
            _framework.Update -= OnFrameworkUpdate;
        }

        public IEnumerable<StatusTrack> EnumerateTracks()
        {
            return _tracks.Values;
        }

        private void OnFrameworkUpdate(Framework framework)
        {
            var partyHash = 17;
            foreach (var member in _partyList)
            {
                unchecked
                {
                    partyHash = partyHash * 23 + (int)member.ClassJob.Id;
                }
            }

            if (_lastPartyHash != partyHash)
            {
                _lastPartyHash = partyHash;

                PluginLog.Debug("Party hash updated, clearing and adding tracks.");
                _tracks.Clear();
                // AddCurrentPartyStatuses();

                foreach (var track in _statusLibrary.InstantiatePlayerTracks(_clientState.LocalPlayer!))
                {
                    _tracks[track.Identifier] = track;
                }
            }
        }

        private void AddCurrentPartyStatuses()
        {
            foreach (var track in _statusLibrary.InstantiatePartyTracks(_partyList))
            {
                if (track.source.objectId == _clientState.LocalPlayer?.ObjectId)
                {
                    PluginLog.Debug($"Skipping track {track} - player's own");
                    continue;
                }

                _tracks[track.Identifier] = track;
            }
        }

        private void OnStatusDetected(Status status)
        {
            if (!_statusLibrary.ShouldTrack(status))
            {
                return;
            }

            if (!_tracks.TryGetValue(StatusTrack.CalculateIdentifier(status.SourceID, status.StatusId), out var track))
            {
                PluginLog.Debug($"Skipping status {status.GameData.Name} - not instantiated previously");
                return;
            }

            track.Update(status.RemainingTime);
        }
    }
}