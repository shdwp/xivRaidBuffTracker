using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Network;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel;
using Ninject;
using RaidBuffTracker.Tracker;
using RaidBuffTracker.Tracker.Library;
using RaidBuffTracker.UI;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace RaidBuffTracker
{
    public sealed class App : IDisposable
    {
        private readonly Configuration          _configuration;
        private readonly TrackerWidget          _widget;
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly CommandManager         _commandManager;
        private readonly GameNetwork            _gameNetwork;
        private readonly ObjectTable            _objectTable;
        private readonly ClientState            _clientState;
        private readonly ActionLibrary          _actionLibrary;
        private readonly ExcelSheet<Action>?    _actionSheet;
        private readonly PartyList              _partyList;
        private readonly GameGui                _gameGui;
        private readonly ChatGui                _chatGui;
        private readonly Framework              _framework;

        private readonly WindowSystem        _windowSystem;
        private readonly ConfigurationWindow _configurationWindow;
        private readonly TrackerWidget       _trackerWidget;

        private static string commandName = "/praidbuff";

        public App(TrackerWidget widget, DalamudPluginInterface pluginInterface, CommandManager commandManager, DataManager dataManager, GameNetwork gameNetwork, ObjectTable objectTable, ConfigurationWindow configurationWindow, TrackerWidget trackerWidget, Configuration configuration, ActionLibrary actionLibrary, PartyList partyList, GameGui gameGui, ChatGui chatGui, Framework framework)
        {
            _widget = widget;
            _pluginInterface = pluginInterface;
            _commandManager = commandManager;
            _gameNetwork = gameNetwork;
            _objectTable = objectTable;
            _configurationWindow = configurationWindow;
            _trackerWidget = trackerWidget;
            _configuration = configuration;
            _actionLibrary = actionLibrary;
            _partyList = partyList;
            _gameGui = gameGui;
            _chatGui = chatGui;
            _framework = framework;

            _configuration.EnabledActions ??= new List<string>(_actionLibrary.AllRecords().Where(r => r.category == ActionCategory.OffensivePrimary).Select(r => r.name));

            _trackerWidget.SetTracker(Module.shared.Get<IActionTracker>(ActionTracker.regular));

            _commandManager.AddHandler(commandName, new CommandInfo(OnCommand));
            _pluginInterface.UiBuilder.Draw += OnUIDraw;
            _gameNetwork.NetworkMessage += OnNetworkMessage;
            _actionSheet = dataManager.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>();

            Module.shared.Inject(_configurationWindow);

            _windowSystem = new WindowSystem();
            _windowSystem.AddWindow(_configurationWindow);
        }

        public void Dispose()
        {
            _gameNetwork.NetworkMessage -= OnNetworkMessage;
            _commandManager.RemoveHandler(commandName);
            _pluginInterface.UiBuilder.Draw -= OnUIDraw;
        }

        private void OnUIDraw()
        {
            _widget.Draw();
            _windowSystem.Draw();
        }

        private unsafe void OnCommand(string command, string arguments)
        {
            arguments = arguments.Trim();

            if (arguments.Length == 0)
            {
                _configurationWindow.Toggle();
            }
            else if (arguments == "dbg")
            {
                var partyList = (AddonPartyList*)_gameGui.GetAddonByName("_PartyList", 1);
                for (var i = 0; i < partyList->MemberCount; i++)
                {
                    var partyMember = partyList->PartyMember[i];
                    var nameNode = partyMember.Name;
                    var slot = partyMember.GroupSlotIndicator;

                    _chatGui.Print(slot->NodeText.ToString() + nameNode->NodeText.ToString());
                }

                var hud = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentHUD();
                for (var i = 0; i < hud->PartyMemberCount; i++)
                {
                    var list = (HudPartyMember*)hud->PartyMemberList;
                    var ptr = list[0];

                    PluginLog.Warning("object " + ptr.ObjectId);
                }
            }
        }

        private void OnNetworkMessage(IntPtr dataptr, ushort opcode, uint sourceactorid, uint targetactorid, NetworkMessageDirection direction)
        {
            /*
            var ignored = new int[] { 689, 887, 534, 507, 722, 962, 441, 454, 393, 970, 926, 706, 516, 603, 909, };
            if (ignored.Contains(opcode))
            {
                return;
            }

            var obj = _objectTable.FirstOrDefault(o => o.ObjectId == targetactorid);

            using var stream = new UnmanagedMemoryStream((byte*)dataptr.ToPointer(), 128);
            stream.Seek(0x8, SeekOrigin.Begin);
            var actionIdBuf = new byte[4];
            stream.Read(actionIdBuf);

            var actionId = BitConverter.ToInt32(actionIdBuf);
            var action = _actionSheet.GetRow((uint)actionId);

            var buf = new byte[128];
            stream.Read(buf);
            File.WriteAllBytes($"./investig/{direction}_{opcode}_{sourceactorid}_{targetactorid}_{DateTime.Now.TimestampSeconds()}", buf);

            PluginLog.Warning($"{direction} OPCODE: {opcode} TARGET: {targetactorid}/{obj?.Name} ACTION: {actionId}/{action?.Name}");
            */
        }
    }
}