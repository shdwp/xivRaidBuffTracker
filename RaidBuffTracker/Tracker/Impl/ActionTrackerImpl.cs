using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Party;
using Dalamud.Logging;
using RaidBuffTracker.Tracker.Library;
using RaidBuffTracker.Tracker.Source;
using RaidBuffTracker.Tracker.Track;
using RaidBuffTracker.Utils;

namespace RaidBuffTracker.Tracker.Impl
{
    public sealed class ActionTrackerImpl : IActionTracker, IDisposable
    {
        private readonly ActionLibrary        _actionLibrary;
        private readonly IActionTrackerSource _source;
        private readonly PartyList            _partyList;
        private readonly PartyListHUD         _partyListHud;
        private readonly Framework            _framework;
        private readonly ClientState          _clientState;
        private readonly Condition            _condition;

        private int _lastPartyHash = 0;

        private readonly List<ActionTrack> _tracks = new();

        public ActionTrackerImpl(ActionLibrary actionLibrary, IActionTrackerSource source, PartyList partyList, Framework framework, ClientState clientState, Condition condition, PartyListHUD partyListHud)
        {
            _actionLibrary = actionLibrary;
            _source = source;
            _partyList = partyList;
            _framework = framework;
            _clientState = clientState;
            _condition = condition;
            _partyListHud = partyListHud;

            _framework.Update += OnFrameworkUpdate;
            _source.ActionInvocationDetected += OnActionDetected;
        }

        public void Dispose()
        {
            _source.ActionInvocationDetected -= OnActionDetected;
            _framework.Update -= OnFrameworkUpdate;
        }

        public IEnumerable<ActionTrack> EnumerateTracks()
        {
            return _tracks;
        }

        public bool AnyTracks()
        {
            return _tracks.Any();
        }

        private void OnFrameworkUpdate(Framework framework)
        {
            if (_condition[ConditionFlag.BetweenAreas]
                || _condition[ConditionFlag.BetweenAreas51])
            {
                return;
            }

            var partyHash = 17;
            foreach (var member in _partyList)
            {
                unchecked
                {
                    partyHash = partyHash * 23 + (int)member.ClassJob.Id;
                }
            }

            unchecked
            {
                partyHash = partyHash * 23 + (int)(_clientState.LocalPlayer?.ClassJob.Id ?? 0);
            }

            if (_lastPartyHash != partyHash)
            {
                _lastPartyHash = partyHash;

                PluginLog.Debug("Party hash updated, clearing and adding tracks.");
                _tracks.Clear();

                Diag.MeasureTime("Adding tracks", () =>
                {
                    AddCurrentPartyTracks();
                    AddLocalPlayerTracks();
                });
            }
        }

        private void AddCurrentPartyTracks()
        {
            var members = _partyList.ToList();
            var order = _partyListHud.GetPartyObjectIDs().ToArray();
            members.Sort((a, b) =>
            {
                var aIndex = Array.FindIndex(order, i => i == a.ObjectId);
                var bIndex = Array.FindIndex(order, i => i == b.ObjectId);

                return aIndex.CompareTo(bIndex);
            });

            foreach (var track in _actionLibrary.InstantiatePartyTracks(members))
            {
                if (track.source.objectId == _clientState.LocalPlayer?.ObjectId)
                {
                    PluginLog.Debug($"Skipping track {track} - player's own");
                    continue;
                }

                PluginLog.Debug($"Adding track {track} - party member");
                _tracks.Add(track);
            }
        }

        private void AddLocalPlayerTracks()
        {
            foreach (var track in _actionLibrary.InstantiatePlayerTracks(_clientState.LocalPlayer!))
            {
                PluginLog.Debug($"Adding track {track} - local player");
                _tracks.Add(track);
            }
        }

        private void OnActionDetected(uint sourceId, uint actionId)
        {
            foreach (var tr in _tracks)
            {
                if (tr.source.objectId == sourceId)
                {
                    tr.UpdateWithInvocation(actionId);
                }
            }
        }
    }
}