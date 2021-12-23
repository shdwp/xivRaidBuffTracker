﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud.Data;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Network;
using Dalamud.Logging;
using Lumina.Excel;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace RaidBuffTracker.Tracker.Source
{
    public sealed class NetworkActionTrackerSource : IActionTrackerSource, IDisposable
    {
        public event Action<uint, uint>? ActionInvocationDetected;

        private readonly GameNetwork _gameNetwork;
        private readonly DataManager _dataManager;
        private readonly ObjectTable _objectTable;

        private readonly ExcelSheet<Action> _actionSheet;

        private readonly int[] _actionOpcodes = { 464, 688 };

        public NetworkActionTrackerSource(GameNetwork gameNetwork, DataManager dataManager, ObjectTable objectTable)
        {
            _gameNetwork = gameNetwork;
            _dataManager = dataManager;
            _objectTable = objectTable;
            _gameNetwork.NetworkMessage += OnNetworkMessage;

            _actionSheet = _dataManager.GameData.GetExcelSheet<Action>();
        }

        public void Dispose()
        {
            _gameNetwork.NetworkMessage -= OnNetworkMessage;
        }

        private void OnNetworkMessage(IntPtr dataptr, ushort opcode, uint sourceactorid, uint targetactorid, NetworkMessageDirection direction)
        {
            if (direction != NetworkMessageDirection.ZoneDown)
            {
                return;
            }

            if (opcode == 0x1d0 || opcode == 0x2b0 || opcode == 0x2be || opcode == 0x288 || opcode == 0x238)
            {
                ProcessAbilityPacket(dataptr, targetactorid);
            }
        }

        private void ProcessAbilityPacket(IntPtr dataPtr, uint actorId)
        {
            var actionId = (uint)Marshal.ReadInt32(dataPtr, 0x8);

            if (false)
            {
                var action = _actionSheet.GetRow(actionId);
                var actor = _objectTable.FirstOrDefault(o => o.ObjectId == actorId);

                PluginLog.Warning($"ACTION USE: {actor?.Name}/{action?.Name}");
            }

            ActionInvocationDetected?.Invoke(actorId, actionId);
        }

        private void ProcessCastPacket(IntPtr dataPtr, uint actorId)
        {
            var actionId = (uint)Marshal.ReadInt16(dataPtr, 0x0);

            if (true)
            {
                var action = _actionSheet.GetRow(actionId);
                var actor = _objectTable.FirstOrDefault(o => o.ObjectId == actorId);

                PluginLog.Warning($"CAST USE: {actor?.Name}/{action?.Name} ({actionId})");
            }
        }
    }
}