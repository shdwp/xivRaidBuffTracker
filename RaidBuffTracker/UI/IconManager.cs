using System;
using System.Collections.Generic;
using Dalamud.Data;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Utility;
using ImGuiScene;
using Lumina.Excel.GeneratedSheets;
using RaidBuffTracker.Tracker.Library;
using RaidBuffTracker.Utils;

namespace RaidBuffTracker.UI
{
    public sealed class IconManager
    {
        private readonly ActionLibrary          _actionLibrary;
        private readonly DataManager            _dataManager;
        private readonly DalamudPluginInterface _pluginInterface;

        private Dictionary<string, TextureWrap> _recordIcons = new();
        private Dictionary<uint, TextureWrap>   _jobIcons    = new();
        private TextureWrap                     _fallbackIcon;

        private static int[] _jobIconIds =
        {
            62101, 62102, 62103, 62104, 62105, 62106, 62107, 62108, 62109, 62110,
            62111, 62112, 62113, 62114, 62115, 62116, 62117, 62118, 62119, 62120,
            62121, 62122, 62123, 62124, 62125, 62126, 62127, 62128, 62129, 62130,
            62131, 62132, 62133, 62134, 62135, 62136, 62137, 62138, 62139, 62140,
        };


        public IconManager(ActionLibrary actionLibrary, DataManager dataManager, DalamudPluginInterface pluginInterface)
        {
            _actionLibrary = actionLibrary;
            _dataManager = dataManager;
            _pluginInterface = pluginInterface;
            _fallbackIcon = LoadIcon(060311);
            Diag.Assert(_fallbackIcon != null, "failed to load fallback icon");

            foreach (var record in _actionLibrary.AllRecords())
            {
                LoadRecordIcon(record);
            }

            var sheet = _dataManager.GameData.GetExcelSheet<ClassJob>();
            foreach (var job in sheet)
            {
                if (job.RowId == 0)
                {
                    continue;
                }

                LoadJobIcon(job.RowId);
            }
        }

        public TextureWrap GetRecordIcon(ActionLibraryRecord record)
        {
            if (_recordIcons.TryGetValue(record.name, out var icon))
            {
                return icon;
            }

            return _fallbackIcon;
        }

        public TextureWrap GetJobIcon(uint jobId)
        {
            if (_jobIcons.TryGetValue(jobId, out var icon))
            {
                return icon;
            }

            return _fallbackIcon;
        }

        private void LoadRecordIcon(ActionLibraryRecord record)
        {
            var texWrap = LoadIcon(record.iconId);
            if (texWrap == null)
            {
                PluginLog.Warning($"Failed to load job icon for {record} ({record.iconId})");
                return;
            }

            _recordIcons[record.name] = texWrap;
        }

        private void LoadJobIcon(uint jobId)
        {
            Diag.Assert(jobId != 0, "invalid job id");

            if (!_jobIcons.ContainsKey(jobId))
            {
                var index = jobId - 1;
                if (index < 0 || index >= _jobIconIds.Length)
                {
                    PluginLog.Warning($"Failed to load job icon for ({jobId}) - no icon id in the array!");
                    return;
                }

                var texWrap = LoadIcon((uint)_jobIconIds[index]);
                if (texWrap == null)
                {
                    PluginLog.Warning($"Failed to load job icon for ({jobId})");
                    return;
                }

                _jobIcons[jobId] = texWrap;
            }
        }

        private TextureWrap? LoadIcon(uint id)
        {
            var tex = _dataManager.GetIcon(true, id);
            if (tex == null)
            {
                PluginLog.Warning($"Failed to load icon {id} - get icon failed");
                return null;
            }

            var texWrap = _pluginInterface.UiBuilder.LoadImageRaw(tex.GetRgbaImageData(), tex.Header.Width, tex.Header.Height, 4);
            if (texWrap.ImGuiHandle == IntPtr.Zero)
            {
                PluginLog.Warning($"Failed to load icon {id} - imgui handle zero");
                texWrap.Dispose();
                return null;
            }

            return texWrap;
        }
    }
}