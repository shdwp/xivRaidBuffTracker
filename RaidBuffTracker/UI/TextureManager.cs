using System;
using System.Collections.Generic;
using Dalamud.Data;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiScene;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using RaidBuffTracker.Tracker.Library;
using RaidBuffTracker.Utils;
using ActionCategory = RaidBuffTracker.Tracker.Library.ActionCategory;

namespace RaidBuffTracker.UI;

public sealed class TextureManager
{
    public TextureManager(ActionLibrary actionLibrary, DataManager dataManager, DalamudPluginInterface pluginInterface)
    {
        _actionLibrary = actionLibrary;
        _dataManager = dataManager;
        _pluginInterface = pluginInterface;
        _fallbackIcon = LoadIcon(60311U);
        Diag.Assert(_fallbackIcon != null, "failed to load fallback icon");
        actionFrame = LoadTexture("ui/uld/icona_frame_hr1.tex");
        Diag.Assert(actionFrame != null, "failed to load acton fram tex");
        actionCooldownSheet = LoadTexture("ui/uld/icona_recast_hr1.tex");
        Diag.Assert(actionCooldownSheet != null, "failed to load action cooldown sheet tex");
        foreach (ActionLibraryRecord record in _actionLibrary.AllRecords())
        {
            LoadRecordIcon(record);
        }

        ExcelSheet<ClassJob> sheet = _dataManager.GameData.GetExcelSheet<ClassJob>();
        foreach (ClassJob job in sheet)
        {
            bool flag = job.RowId == 0U;
            if (!flag)
            {
                LoadJobIcon(job.RowId);
            }
        }

        foreach (ActionCategory category in Enum.GetValues<ActionCategory>())
        {
            LoadCategoryIcon(category);
        }
    }

    public TextureWrap GetRecordIcon(ActionLibraryRecord record)
    {
        TextureWrap icon;
        bool flag = _recordIcons.TryGetValue(record.name, out icon);
        TextureWrap result;
        if (flag)
        {
            result = icon;
        }
        else
        {
            result = _fallbackIcon;
        }

        return result;
    }

    public TextureWrap GetJobIcon(uint jobId)
    {
        TextureWrap icon;
        bool flag = _jobIcons.TryGetValue(jobId, out icon);
        TextureWrap result;
        if (flag)
        {
            result = icon;
        }
        else
        {
            result = _fallbackIcon;
        }

        return result;
    }

    public TextureWrap GetCategoryIcon(ActionCategory category)
    {
        TextureWrap icon;
        bool flag = _caterogyIcons.TryGetValue(category, out icon);
        TextureWrap result;
        if (flag)
        {
            result = icon;
        }
        else
        {
            result = _fallbackIcon;
        }

        return result;
    }

    private void LoadRecordIcon(ActionLibraryRecord record)
    {
        TextureWrap texWrap = LoadIcon(record.iconId);
        bool flag = texWrap == null;
        if (flag)
        {
            PluginLog.Warning(string.Format("Failed to load job icon for {0} ({1})", record, record.iconId),
                Array.Empty<object>());
        }
        else
        {
            _recordIcons[record.name] = texWrap;
        }
    }

    private void LoadJobIcon(uint jobId)
    {
        Diag.Assert(jobId > 0U, "invalid job id");
        bool flag = !_jobIcons.ContainsKey(jobId);
        if (flag)
        {
            uint index = jobId - 1U;
            bool flag2 = index < 0U || index >= (ulong)_jobIconIds.Length;
            if (flag2)
            {
                PluginLog.Warning(string.Format("Failed to load job icon for ({0}) - no icon id in the array!", jobId),
                    Array.Empty<object>());
            }
            else
            {
                TextureWrap texWrap = LoadIcon((uint)_jobIconIds[(int)index]);
                bool flag3 = texWrap == null;
                if (flag3)
                {
                    PluginLog.Warning(string.Format("Failed to load job icon for ({0})", jobId), Array.Empty<object>());
                }
                else
                {
                    _jobIcons[jobId] = texWrap;
                }
            }
        }
    }

    private void LoadCategoryIcon(ActionCategory category)
    {
        uint iconId = UICommon.GetActionCategoryIcon(category);
        TextureWrap wrap = LoadIcon(iconId);
        bool flag = wrap == null;
        if (flag)
        {
            PluginLog.Warning(string.Format("Failed to load category icon for {0} ({1})", category, iconId),
                Array.Empty<object>());
        }
        else
        {
            _caterogyIcons[category] = wrap;
        }
    }

    private TextureWrap LoadIcon(uint id)
    {
        TextureWrap texWrap = _dataManager.GetImGuiTextureHqIcon(id);
        bool flag = texWrap == null || texWrap.ImGuiHandle == IntPtr.Zero;
        TextureWrap result;
        if (flag)
        {
            PluginLog.Warning(string.Format("Failed to load icon {0} - imgui handle zero", id), Array.Empty<object>());
            result = null;
        }
        else
        {
            result = texWrap;
        }

        return result;
    }

    private TextureWrap LoadTexture(string path)
    {
        TextureWrap tex = _dataManager.GetImGuiTexture(path);
        bool flag = tex == null || tex.ImGuiHandle == IntPtr.Zero;
        TextureWrap result;
        if (flag)
        {
            PluginLog.Warning("Failed to load texture " + path + " - failed", Array.Empty<object>());
            result = null;
        }
        else
        {
            result = tex;
        }

        return result;
    }

    private readonly ActionLibrary _actionLibrary;
    private readonly DataManager _dataManager;
    private readonly DalamudPluginInterface _pluginInterface;
    public TextureWrap actionFrame;

    // Token: 0x04000089 RID: 137
    public TextureWrap actionCooldownSheet;

    // Token: 0x0400008A RID: 138
    private Dictionary<string, TextureWrap> _recordIcons = new Dictionary<string, TextureWrap>();

    // Token: 0x0400008B RID: 139
    private Dictionary<uint, TextureWrap> _jobIcons = new Dictionary<uint, TextureWrap>();

    // Token: 0x0400008C RID: 140
    private Dictionary<ActionCategory, TextureWrap> _caterogyIcons = new Dictionary<ActionCategory, TextureWrap>();

    // Token: 0x0400008D RID: 141
    private TextureWrap _fallbackIcon;

    // Token: 0x0400008E RID: 142
    private static int[] _jobIconIds =
    {
        62101,
        62102,
        62103,
        62104,
        62105,
        62106,
        62107,
        62108,
        62109,
        62110,
        62111,
        62112,
        62113,
        62114,
        62115,
        62116,
        62117,
        62118,
        62119,
        62120,
        62121,
        62122,
        62123,
        62124,
        62125,
        62126,
        62127,
        62128,
        62129,
        62130,
        62131,
        62132,
        62133,
        62134,
        62135,
        62136,
        62137,
        62138,
        62139,
        62140
    };
}