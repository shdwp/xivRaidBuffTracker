using System;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using RaidBuffTracker.Tracker.Library;
using RaidBuffTracker.Tracker.Track;
using RaidBuffTracker.Utils;

namespace RaidBuffTracker.Tracker
{
    public sealed class MockStatusTrackerImpl : IStatusTracker, IDisposable
    {
        private readonly Framework     _framework;
        private readonly StatusLibrary _statusLibrary;
        private readonly ClientState   _clientState;

        private readonly StatusTrack _activeTrack;
        private readonly StatusTrack _readyTrack;
        private readonly StatusTrack _cooldownTrack;

        public MockStatusTrackerImpl(StatusLibrary statusLibrary, ClientState clientState, Framework framework)
        {
            _statusLibrary = statusLibrary;
            _clientState = clientState;
            _framework = framework;

            _activeTrack = _statusLibrary.InstantiateTrack(_clientState.LocalPlayer, _statusLibrary.FindRecord(1228));
            _readyTrack = _statusLibrary.InstantiateTrack(_clientState.LocalPlayer, _statusLibrary.FindRecord(1228));
            _cooldownTrack = _statusLibrary.InstantiateTrack(_clientState.LocalPlayer, _statusLibrary.FindRecord(1228));

            _framework.Update += OnUpdate;
        }

        public void Dispose()
        {
            _framework.Update -= OnUpdate;
        }

        private void OnUpdate(Framework framework)
        {
            _activeTrack.lastInvoked = DateTime.Now.TimestampSeconds() - 15.0;
            _readyTrack.lastInvoked = DateTime.Now.TimestampSeconds() - 70.0;
            _cooldownTrack.lastInvoked = DateTime.Now.TimestampSeconds() - 50.0;
        }

        public IEnumerable<StatusTrack> EnumerateTracks()
        {
            yield return _activeTrack;
            yield return _readyTrack;
            yield return _cooldownTrack;
        }
    }
}