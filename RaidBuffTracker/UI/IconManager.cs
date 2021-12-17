using System;
using System.Collections.Generic;
using Dalamud.Data;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Utility;
using ImGuiScene;
using RaidBuffTracker.Tracker.Library;

namespace RaidBuffTracker.UI
{
    public sealed class IconManager
    {
        private readonly StatusLibrary          _statusLibrary;
        private readonly DataManager            _dataManager;
        private readonly DalamudPluginInterface _pluginInterface;

        private Dictionary<uint, TextureWrap> _statusIcons = new();

        public IconManager(StatusLibrary statusLibrary, DataManager dataManager, DalamudPluginInterface pluginInterface)
        {
            _statusLibrary = statusLibrary;
            _dataManager = dataManager;
            _pluginInterface = pluginInterface;

            foreach (var record in _statusLibrary.AllRecords())
            {
                var tex = _dataManager.GetIcon(record.iconId);
                if (tex == null)
                {
                    PluginLog.Warning($"Failed to load icon {record.iconId} for {record} - tex null");
                    continue;
                }

                var texWrap = _pluginInterface.UiBuilder.LoadImageRaw(tex.GetRgbaImageData(), tex.Header.Width, tex.Header.Height, 4);
                if (texWrap.ImGuiHandle == IntPtr.Zero)
                {
                    PluginLog.Warning($"Failed to load icon {record.iconId} for {record} - imgui handle zero");
                    texWrap.Dispose();
                    continue;
                }

                _statusIcons[record.statusId] = texWrap;
            }
        }

        public TextureWrap? GetIcon(StatusLibraryRecord @record)
        {
            return _statusIcons.GetValueOrDefault(@record.statusId);
        }
    }
}