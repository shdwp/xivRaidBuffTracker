using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ImGuiNET;
using Ninject;
using RaidBuffTracker.Tracker;
using RaidBuffTracker.Tracker.Impl;
using RaidBuffTracker.Tracker.Library;
using RaidBuffTracker.Tracker.Track;
using RaidBuffTracker.UI.Layout;

namespace RaidBuffTracker.UI;

public sealed class ConfigurationWindow : Window
{
    // Token: 0x06000064 RID: 100 RVA: 0x00002AC3 File Offset: 0x00000CC3
    public ConfigurationWindow(TrackerWidget trackerWidget, Configuration configuration,
        DalamudPluginInterface pluginInterface, ActionLibrary actionLibrary, MockActionTrackerImpl mockTracker,
        TextureManager textureManager) : base("RaidBuffTracker configuration")
    {
        _trackerWidget = trackerWidget;
        _configuration = configuration;
        _pluginInterface = pluginInterface;
        _actionLibrary = actionLibrary;
        _mockTracker = mockTracker;
        _textureManager = textureManager;
    }

    // Token: 0x06000065 RID: 101 RVA: 0x00002B01 File Offset: 0x00000D01
    public override void OnOpen()
    {
        base.OnOpen();
        _trackerWidget.isLocked = false;

        _trackerWidget.SetTracker(Module.shared.Get<IActionTracker>(ActionTracker.testingDisplay));
    }

    // Token: 0x06000066 RID: 102 RVA: 0x00002B37 File Offset: 0x00000D37
    public override void OnClose()
    {
        base.OnClose();
        _trackerWidget.isLocked = true;
        SaveConfiguration();
        _trackerWidget.SetTracker(Module.shared.Get<IActionTracker>(ActionTracker.regular));
    }

    // Token: 0x06000067 RID: 103 RVA: 0x00002B74 File Offset: 0x00000D74
    public override void Draw()
    {
        StringBuilder mockedActions = new StringBuilder();
        foreach (ActionTrack track in _mockTracker.EnumerateTracks())
        {
            mockedActions.Append(track.record.name + ", ");
        }

        ImGui.TextWrapped(
            string.Format("When configuration window is opened following actions activate: {0}. ",
                mockedActions.Remove(mockedActions.Length - 2, 2)) +
            "Based on configuration you may or may not see them. ");
        bool flag = ImGui.BeginTabBar("##tabbar");
        if (flag)
        {
            bool flag2 = ImGui.BeginTabItem("General##general");
            if (flag2)
            {
                DrawGeneralTab();
                ImGui.EndTabItem();
            }

            bool flag3 = ImGui.BeginTabItem("Actions##actions");
            if (flag3)
            {
                DrawActionsTab();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    // Token: 0x06000068 RID: 104 RVA: 0x00002C64 File Offset: 0x00000E64
    private void DrawGeneralTab()
    {
        bool widgetInteraction = _configuration.MouseInteraction;
        bool flag = ImGui.Checkbox("Widget mouse interaction##widget_interaction", ref widgetInteraction);
        if (flag)
        {
            _configuration.MouseInteraction = widgetInteraction;
            SaveConfiguration();
        }

        ImGuiHelpTooltip(
            "Enables widget mouse interaction - like tooltips for actions, for example, but blocks mouse input.\nDisable this to click through the widget.");
        bool showOnlyInInstancesValue = _configuration.OnlyInInstances;
        bool flag2 = ImGui.Checkbox("Show only in instances##show_only_in_instances", ref showOnlyInInstancesValue);
        if (flag2)
        {
            _configuration.OnlyInInstances = showOnlyInInstancesValue;
            SaveConfiguration();
        }

        bool showPlayerActions = _configuration.ShowPlayerActions;
        bool flag3 = ImGui.Checkbox("Show player own actions##show_own_actions", ref showPlayerActions);
        if (flag3)
        {
            _configuration.ShowPlayerActions = showPlayerActions;
            SaveConfiguration();
        }

        bool slitIntoCategories = _configuration.SplitIntoCategories;
        bool flag4 = ImGui.Checkbox("Split into categories##slit_into_categories", ref slitIntoCategories);
        if (flag4)
        {
            _configuration.SplitIntoCategories = slitIntoCategories;
            SaveConfiguration();
        }

        ImGuiHelpTooltip(
            "While actions are still be sorted by categories, this option also enables separator icons in between categories.");
        bool showBottomIcons = _configuration.BottomIconsEnabled;
        bool flag5 = ImGui.Checkbox("Show bottom icons##show_bottom_icons", ref showBottomIcons);
        if (flag5)
        {
            _configuration.BottomIconsEnabled = showBottomIcons;
            SaveConfiguration();
        }

        ImGuiHelpTooltip("Show or hide bottom icons (job icon and party slot number).");
        bool showActiveCountdown = _configuration.ShowActiveCountdown;
        bool flag6 = ImGui.Checkbox("Show active countdown##show_active_countdown", ref showActiveCountdown);
        if (flag6)
        {
            _configuration.ShowActiveCountdown = showActiveCountdown;
            SaveConfiguration();
        }

        ImGuiHelpTooltip("Show or hide effect active time remaining.");
        ImGui.Text("Layout: ");
        bool flag7 = ImGui.BeginCombo("##widget_layout",
            TrackerWidgetLayout.GetLayoutName(_configuration.WidgetLayout));
        if (flag7)
        {
            foreach (string widgetLayoutId in TrackerWidgetLayout.Enumerate())
            {
                bool flag8 = ImGui.Selectable(TrackerWidgetLayout.GetLayoutName(widgetLayoutId) +
                                              "##widget_layout_combo_" + widgetLayoutId);
                if (flag8)
                {
                    _configuration.WidgetLayout = widgetLayoutId;
                    _trackerWidget.SetLayout(Module.shared.Get<ITrackerWidgetLayout>(widgetLayoutId));
                    SaveConfiguration();
                }
            }

            ImGui.EndCombo();
        }

        ImGui.Text("Item size:");
        float cellSize = _configuration.WidgetCellSize.X;
        bool flag9 = ImGui.SliderFloat("##item_size", ref cellSize, 10f, 250f);
        if (flag9)
        {
            _configuration.WidgetCellSize = new Vector2(cellSize, cellSize);
            SaveConfiguration();
        }
    }

    // Token: 0x06000069 RID: 105 RVA: 0x00002F00 File Offset: 0x00001100
    private void DrawActionsTab()
    {
        bool flag = _configuration.EnabledActions == null;
        if (flag)
        {
            throw new InvalidDataException("Configuration invalid - EnableActions null.");
        }

        DrawActionsCategory(ActionCategory.OffensivePrimary);
        ImGui.Dummy(new Vector2(0f, 25f));
        ImGui.Separator();
        DrawActionsCategory(ActionCategory.OffensiveSecondary);
        ImGui.Dummy(new Vector2(0f, 25f));
        ImGui.Separator();
        DrawActionsCategory(ActionCategory.MitigationPrimary);
        ImGui.Dummy(new Vector2(0f, 25f));
        ImGui.Separator();
        DrawActionsCategory(ActionCategory.MitigationSecondary);
        ImGui.Dummy(new Vector2(0f, 25f));
        ImGui.Separator();
        DrawActionsCategory(ActionCategory.Utility);
    }

    // Token: 0x0600006A RID: 106 RVA: 0x00002FC0 File Offset: 0x000011C0
    private void DrawActionsCategory(ActionCategory category)
    {
        ImGui.Text(UICommon.GetActionCategoryName(category) + ": ");
        ImGui.SameLine();
        bool flag = ImGui.Button("All##category_all_" + category);
        if (flag)
        {
            foreach (KeyValuePair<ActionLibraryRecord, bool> kv in EnumerateActionsInCategory(category))
            {
                bool flag2 = !kv.Value;
                if (flag2)
                {
                    _configuration.EnabledActions.Add(kv.Key.name);
                }
            }

            SaveConfiguration();
        }

        ImGui.SameLine();
        bool flag3 = ImGui.Button("None##category_all_" + category);
        if (flag3)
        {
            foreach (KeyValuePair<ActionLibraryRecord, bool> kv2 in EnumerateActionsInCategory(category))
            {
                _configuration.EnabledActions.Remove(kv2.Key.name);
            }

            SaveConfiguration();
        }

        foreach (KeyValuePair<ActionLibraryRecord, bool> kv3 in EnumerateActionsInCategory(category))
        {
            ActionLibraryRecord record = kv3.Key;
            bool enabled = kv3.Value;
            bool flag4 = ImGui.Checkbox("##" + record.name + "_toggle", ref enabled);
            if (flag4)
            {
                bool flag5 = enabled;
                if (flag5)
                {
                    _configuration.EnabledActions.Add(record.name);
                }
                else
                {
                    _configuration.EnabledActions.Remove(record.name);
                }

                SaveConfiguration();
            }

            ImGui.SameLine();
            ImGui.Image(_textureManager.GetRecordIcon(kv3.Key).ImGuiHandle,
                new Vector2(20f, 20f) * ImGui.GetIO().FontGlobalScale);
            ImGui.SameLine();
            ImGui.Text(record.name);
            ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.DalamudGrey2, record.tooltip);
            ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.DalamudGrey3,
                string.Format("{0} lvl.{1}", record.jobAffinity, record.minLvl));
        }
    }

    private IEnumerable<KeyValuePair<ActionLibraryRecord, bool>> EnumerateActionsInCategory(ActionCategory category)
    {
        foreach (ActionLibraryRecord record in _actionLibrary.AllRecords().Where(r => r.category == category))
        {
            bool enabled = _configuration.EnabledActions.Contains(record.name);
            yield return new KeyValuePair<ActionLibraryRecord, bool>(record, enabled);
        }
    }

    private void ImGuiHelpTooltip(string tooltip)
    {
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.8f, 1f), "?");
        bool flag = ImGui.IsItemHovered();
        if (flag)
        {
            ImGui.SetTooltip(tooltip);
        }
    }

    // Token: 0x0600006D RID: 109 RVA: 0x000032F6 File Offset: 0x000014F6
    private void SaveConfiguration()
    {
        _pluginInterface.SavePluginConfig(_configuration);
    }

    // Token: 0x0400007F RID: 127
    private readonly TrackerWidget _trackerWidget;

    // Token: 0x04000080 RID: 128
    private readonly Configuration _configuration;

    // Token: 0x04000081 RID: 129
    private readonly DalamudPluginInterface _pluginInterface;

    // Token: 0x04000082 RID: 130
    private readonly ActionLibrary _actionLibrary;

    // Token: 0x04000083 RID: 131
    private readonly MockActionTrackerImpl _mockTracker;

    // Token: 0x04000084 RID: 132
    private readonly TextureManager _textureManager;
}