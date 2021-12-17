using System;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Logging;
using RaidBuffTracker.Tracker.Library;

namespace RaidBuffTracker.Tracker.Source
{
    public sealed class ObjectTableStatusTrackerSource : IDisposable, IStatusTrackerSource
    {
        public event Action<Status>? StatusDetected;

        private readonly Framework   _framework;
        private readonly ObjectTable _objectTable;
        private readonly Condition   _condition;
        private readonly ClientState _clientState;

        public ObjectTableStatusTrackerSource(Framework framework, ObjectTable objectTable, StatusLibrary statusLibrary, Condition condition, ClientState clientState)
        {
            PluginLog.Debug("ObjectTableSource begin");

            _framework = framework;
            _objectTable = objectTable;
            _condition = condition;
            _clientState = clientState;
            _framework.Update += OnUpdate;
        }

        public void Dispose()
        {
            _framework.Update -= OnUpdate;
            PluginLog.Debug("ObjectTableSource end");
        }

        private void OnUpdate(Framework framework)
        {
            foreach (var obj in _objectTable)
            {
                if (!(obj is BattleChara battleChara))
                {
                    continue;
                }

                foreach (var status in battleChara.StatusList)
                {
                    if (status.SourceObject?.ObjectKind != ObjectKind.Player)
                    {
                        continue;
                    }

                    if (status.SourceID == _clientState.LocalPlayer?.ObjectId)
                    {
                        StatusDetected?.Invoke(status);
                    }
                }
            }
        }
    }
}