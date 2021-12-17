using System;
using Dalamud.Data;
using Dalamud.Game.Command;
using Dalamud.Logging;
using Dalamud.Plugin;
using RaidBuffTracker.UI;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace RaidBuffTracker
{
    public sealed class App : IDisposable
    {
        private readonly TrackerWidget          _widget;
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly CommandManager         _commandManager;
        private readonly DataManager            _dataManager;

        public App(TrackerWidget widget, DalamudPluginInterface pluginInterface, CommandManager commandManager, DataManager dataManager)
        {
            _widget = widget;
            _pluginInterface = pluginInterface;
            _commandManager = commandManager;
            _dataManager = dataManager;

            _commandManager.AddHandler("/prb", new CommandInfo(OnCommand));
            _pluginInterface.UiBuilder.Draw += OnUIDraw;
        }

        public void Dispose()
        {
            _commandManager.RemoveHandler("/prb");
            _pluginInterface.UiBuilder.Draw -= OnUIDraw;
        }

        private void OnUIDraw()
        {
            _widget.Draw();
        }

        private void OnCommand(string command, string arguments)
        {
            var actionSheet = _dataManager.GameData.GetExcelSheet<Action>();
            foreach (var action in actionSheet)
            {
                if (action.Name.ToString() == "Trick Attack")
                {
                    var status = action.ActionProcStatus.Row;
                    PluginLog.Warning($"{action.Name} - {status} {action.ActionProcStatus}");
                }
            }

        }
    }
}