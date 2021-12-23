using System;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using RaidBuffTracker.Tracker.Library;
using RaidBuffTracker.Tracker.Track;
using RaidBuffTracker.Utils;

namespace RaidBuffTracker.Tracker.Impl
{
    public sealed class MockActionTrackerImpl : IActionTracker, IDisposable
    {
        private readonly Framework     _framework;
        private readonly ActionLibrary _actionLibrary;
        private readonly ClientState   _clientState;

        private readonly ActionTrack _activeTrack;
        private readonly ActionTrack _readyTrack;
        private readonly ActionTrack _cooldownTrack;
        private readonly ActionTrack _activeTrack2;
        private readonly ActionTrack _readyTrack2;
        private readonly ActionTrack _cooldownTrack2;

        public MockActionTrackerImpl(ActionLibrary actionLibrary, ClientState clientState, Framework framework)
        {
            _actionLibrary = actionLibrary;
            _clientState = clientState;
            _framework = framework;

            var drg = new ActionTrackSource
            {
                objectId = 1,
                name = "Bad Dragoon",
                index = 1,
                jobAbbr = "DRG",
                jobId = 22,
                lvl = 90,
            };

            var nin = new ActionTrackSource
            {
                objectId = 2,
                name = "Theihe Leihe",
                index = 2,
                jobAbbr = "NIN",
                jobId = 29,
                lvl = 90,
            };

            var brd = new ActionTrackSource
            {
                objectId = 3,
                name = "Valca Namoria",
                index = 3,
                jobAbbr = "BRD",
                jobId = 23,
                lvl = 90,
            };

            var gnb = new ActionTrackSource
            {
                objectId = 4,
                name = "Pylyp Orlyk",
                index = 4,
                jobAbbr = "GNB",
                jobId = 37,
                lvl = 90,
            };

            var sam = new ActionTrackSource
            {
                objectId = 5,
                name = "Nameless Samurai",
                index = 5,
                jobAbbr = "SAM",
                jobId = 30,
                lvl = 90,
            };

            _activeTrack = _actionLibrary.InstantiateTrack(drg, _actionLibrary.FindRecord("Battle Litany"));
            _readyTrack = _actionLibrary.InstantiateTrack(nin, _actionLibrary.FindRecord("Trick Attack"));
            _cooldownTrack = _actionLibrary.InstantiateTrack(brd, _actionLibrary.FindRecord("Battle Voice"));
            _activeTrack2 = _actionLibrary.InstantiateTrack(brd, _actionLibrary.FindRecord("Mage's Ballad"));
            _readyTrack2 = _actionLibrary.InstantiateTrack(sam, _actionLibrary.FindRecord("Feint"));
            _cooldownTrack2 = _actionLibrary.InstantiateTrack(gnb, _actionLibrary.FindRecord("Reprisal"));

            _framework.Update += OnUpdate;
        }

        public void Dispose()
        {
            _framework.Update -= OnUpdate;
        }

        private void OnUpdate(Framework framework)
        {
            _activeTrack.lastActivated = DateTime.Now.TimestampSeconds() - 10.0;
            _activeTrack.lastInvoked = DateTime.Now.TimestampSeconds() - 20.0;

            _readyTrack.lastActivated = DateTime.Now.TimestampSeconds() - 240.0;
            _readyTrack.lastInvoked = DateTime.Now.TimestampSeconds() - 240.0;

            _cooldownTrack.lastActivated = DateTime.Now.TimestampSeconds() - 40.0;
            _cooldownTrack.lastInvoked = DateTime.Now.TimestampSeconds() - 80.0;

            _activeTrack2.lastActivated = DateTime.Now.TimestampSeconds() - 10.0;
            _activeTrack2.lastInvoked = DateTime.Now.TimestampSeconds() - 20.0;

            _readyTrack2.lastActivated = DateTime.Now.TimestampSeconds() - 240;
            _readyTrack2.lastInvoked = DateTime.Now.TimestampSeconds() - 240;

            _cooldownTrack2.lastActivated = DateTime.Now.TimestampSeconds() - 40.0;
            _cooldownTrack2.lastInvoked = DateTime.Now.TimestampSeconds() - 80.0;
        }

        public IEnumerable<ActionTrack> EnumerateTracks()
        {
            yield return _activeTrack;
            yield return _readyTrack;
            yield return _cooldownTrack;

            yield return _activeTrack2;
            yield return _readyTrack2;
            yield return _cooldownTrack2;
        }

        public bool AnyTracks()
        {
            return true;
        }
    }
}