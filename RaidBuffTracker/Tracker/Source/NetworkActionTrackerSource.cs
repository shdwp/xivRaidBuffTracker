using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Data;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Network;
using Dalamud.Logging;
using Lumina.Excel;
using Newtonsoft.Json;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace RaidBuffTracker.Tracker.Source
{
    public sealed class NetworkActionTrackerSource : IActionTrackerSource, IDisposable
    {
        public event Action<uint, uint>? ActionInvocationDetected;
        public event Action<uint>? ActorZoningDetected;

        private const string OpcodesUrl = "https://raw.githubusercontent.com/karashiiro/FFXIVOpcodes/master/opcodes.min.json";

        private readonly GameNetwork _gameNetwork;
        private readonly DataManager _dataManager;
        private readonly ObjectTable _objectTable;

        private readonly ExcelSheet<Action> _actionSheet;

        private readonly List<int> _actionEffectOpcodes = new();
        private readonly List<int> _prepareZoningOpcodes = new();

        public NetworkActionTrackerSource(GameNetwork gameNetwork, DataManager dataManager, ObjectTable objectTable)
        {
            _gameNetwork = gameNetwork;
            _dataManager = dataManager;
            _objectTable = objectTable;
            _gameNetwork.NetworkMessage += OnNetworkMessage;

            _actionSheet = _dataManager.GameData.GetExcelSheet<Action>();

            Task.WaitAll(Initialize());
        }

        public void Dispose()
        {
            _gameNetwork.NetworkMessage -= OnNetworkMessage;
        }

        private async Task Initialize()
        {
            PluginLog.Debug("Downloading opcodes");
            var client = new HttpClient();
            var data = await client.GetStringAsync(OpcodesUrl);
            dynamic json = JsonConvert.DeserializeObject(data);

            foreach (var clientType in json)
            {
                if (clientType.region == "Global")
                {
                    foreach (var record in clientType["lists"]["ServerZoneIpcType"])
                    {
                        var name = record.name.ToString();
                        var opcode = (int)record.opcode;
                        if (name == "PrepareZoning")
                        {
                            _prepareZoningOpcodes.Add(opcode);
                            PluginLog.Debug($"Adding zoning opcode - {record.name} ({record.opcode})");
                        }
                        else if (name.StartsWith("AoeEffect") || name == "Effect")
                        {
                            _actionEffectOpcodes.Add(opcode);
                            PluginLog.Debug($"Adding action effect opcode - {record.name} ({record.opcode})");
                        }
                    }
                }
            }
        }

        private void OnNetworkMessage(IntPtr dataptr, ushort opcode, uint sourceactorid, uint targetactorid,
            NetworkMessageDirection direction)
        {
            if (direction != NetworkMessageDirection.ZoneDown)
            {
                return;
            }

            if (_actionEffectOpcodes.Contains(opcode))
            {
                ProcessActionPacket(dataptr, opcode, targetactorid);
            }
            else if (_prepareZoningOpcodes.Contains(opcode))
            {
                ProcessZoningPacket(dataptr, targetactorid);
            }
        }

        private void ProcessActionPacket(IntPtr dataPtr, ushort opcode, uint actorId)
        {
            ushort actionId = (ushort)Marshal.ReadInt16(dataPtr, 28);
            uint oldPositionActionID = (uint)Marshal.ReadInt32(dataPtr, 8);
            Action action = _actionSheet.GetRow(actionId);
            Action oldAction = _actionSheet.GetRow(oldPositionActionID);
            GameObject actor = _objectTable.FirstOrDefault(o => o.ObjectId == actorId);
            bool flag = actionId != oldPositionActionID;
            if (flag)
            {
                PluginLog.Warning(
                    string.Format("[{0}] Action IDs of {1} mismatch: {2} ({3}) != {4} ({5})", opcode, (actor != null) ? actor.Name : null, actionId,
                        (action != null) ? action.Name : null, oldPositionActionID, (oldAction != null) ? oldAction.Name : null), Array.Empty<object>());
            }

            Action<uint, uint> actionInvocationDetected = ActionInvocationDetected;
            if (actionInvocationDetected != null)
            {
                actionInvocationDetected(actorId, actionId);
            }
        }

        private void ProcessZoningPacket(IntPtr dataPtr, uint actorId)
        {
            Action<uint> actorZoningDetected = ActorZoningDetected;
            if (actorZoningDetected != null)
            {
                actorZoningDetected(actorId);
            }
        }
    }
}