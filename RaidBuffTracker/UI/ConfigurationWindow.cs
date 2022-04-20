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

namespace RaidBuffTracker.UI
{
    public sealed class ConfigurationWindow : Window
    {
        private readonly TrackerWidget          _trackerWidget;
        private readonly Configuration          _configuration;
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly ActionLibrary          _actionLibrary;
        private readonly MockActionTrackerImpl  _mockTracker;
        private readonly IconManager            _iconManager;

        public ConfigurationWindow(
            TrackerWidget trackerWidget,
            Configuration configuration,
            DalamudPluginInterface pluginInterface,
            ActionLibrary actionLibrary,
            MockActionTrackerImpl mockTracker,
            IconManager iconManager
        ) : base("RaidBuffTracker configuration")
        {
            _trackerWidget = trackerWidget;
            _configuration = configuration;
            _pluginInterface = pluginInterface;
            _actionLibrary = actionLibrary;
            _mockTracker = mockTracker;
            _iconManager = iconManager;
        }

        public override void OnOpen()
        {
            base.OnOpen();

            _trackerWidget.isLocked = false;
            _trackerWidget.SetTracker(Module.shared.Get<IActionTracker>(ActionTracker.testingDisplay));
        }

        public override void OnClose()
        {
            base.OnClose();

            _trackerWidget.isLocked = true;
            SaveConfiguration();

            _trackerWidget.SetTracker(Module.shared.Get<IActionTracker>(ActionTracker.regular));
        }

        public override void Draw()
        {
            var mockedActions = new StringBuilder();
            foreach (var track in _mockTracker.EnumerateTracks())
            {
                mockedActions.Append($"{track.record.name}, ");
            }

            ImGui.TextWrapped($"When configuration window is opened following actions activate: {mockedActions.Remove(mockedActions.Length - 2, 2)}. " +
                              $"Based on configuration you may or may not see them. " +
                              $"Alpha notice: only actions in the Offensive category were tested.");

            if (ImGui.BeginTabBar("##tabbar"))
            {
                if (ImGui.BeginTabItem("General##general"))
                {
                    DrawGeneralTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Actions##actions"))
                {
                    DrawActionsTab();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        private void DrawGeneralTab()
        {
            var widgetInteraction = _configuration.WidgetInteraction;
            if (ImGui.Checkbox("Widget mouse interaction##widget_interaction", ref widgetInteraction))
            {
                _configuration.WidgetInteraction = widgetInteraction;
                SaveConfiguration();
            }
            ImGuiHelpTooltip("Enables widget mouse interaction - like tooltips for actions, for example, but blocks mouse input.\nDisable this to click through the widget.");

            var showOnlyInInstancesValue = _configuration.OnlyInInstances;
            if (ImGui.Checkbox("Show only in instances##show_only_in_instances", ref showOnlyInInstancesValue))
            {
                _configuration.OnlyInInstances = showOnlyInInstancesValue;
                SaveConfiguration();
            }

            var slitIntoCategories = _configuration.WidgetSplitIntoCategories;
            if (ImGui.Checkbox("Split into categories##slit_into_categories", ref slitIntoCategories))
            {
                _configuration.WidgetSplitIntoCategories = slitIntoCategories;
                SaveConfiguration();
            }
            ImGuiHelpTooltip("Each of the category will start on the separate row.");

            var widgetIcons = _configuration.WidgetIcons;
            if (ImGui.Checkbox("Widget icons##widget_icons", ref widgetIcons))
            {
                _configuration.WidgetIcons = widgetIcons;
                SaveConfiguration();
            }
            ImGuiHelpTooltip("Enables widget icons, such as job and player numbers.");



            var reverseOrder = _configuration.WidgetReverseOrder;
            if (ImGui.Checkbox("Reverse sorting order##reverse_sorting_order", ref reverseOrder))
            {
                _configuration.WidgetReverseOrder = reverseOrder;
                SaveConfiguration();
            }
            ImGuiHelpTooltip("Reverses the order of the actions.");

            ImGui.Text("Item size:");
            var cellSize = _configuration.WidgetCellSize.X;
            if (ImGui.SliderFloat("##item_size", ref cellSize, 65, 400))
            {
                _configuration.WidgetCellSize = new Vector2(cellSize, cellSize);
                SaveConfiguration();
            }
        }

        private void DrawActionsTab()
        {
            if (_configuration.EnabledActions == null)
            {
                throw new InvalidDataException("Configuration invalid - EnableActions null.");
            }

            DrawActionsCategory(ActionCategory.OffensivePrimary);
            ImGui.Dummy(new Vector2(0, 25));
            ImGui.Separator();
            DrawActionsCategory(ActionCategory.OffensiveSecondary);
            ImGui.Dummy(new Vector2(0, 25));
            ImGui.Separator();
            DrawActionsCategory(ActionCategory.MitigationPrimary);
            ImGui.Dummy(new Vector2(0, 25));
            ImGui.Separator();
            DrawActionsCategory(ActionCategory.MitigationSecondary);
            ImGui.Dummy(new Vector2(0, 25));
            ImGui.Separator();
            DrawActionsCategory(ActionCategory.Utility);
        }

        private void DrawActionsCategory(ActionCategory category)
        {
            ImGui.Text(category + ": ");

            ImGui.SameLine();
            if (ImGui.Button("All##category_all_" + category))
            {
                foreach (var kv in EnumerateActionsInCategory(category))
                {
                    if (!kv.Value)
                    {
                        _configuration.EnabledActions!.Add(kv.Key.name);
                    }
                }

                SaveConfiguration();
            }

            ImGui.SameLine();
            if (ImGui.Button("None##category_all_" + category))
            {
                foreach (var kv in EnumerateActionsInCategory(category))
                {
                    _configuration.EnabledActions!.Remove(kv.Key.name);
                }

                SaveConfiguration();
            }

            foreach (var kv in EnumerateActionsInCategory(category))
            {
                var record = kv.Key;
                var enabled = kv.Value;
                if (ImGui.Checkbox($"##{record.name}_toggle", ref enabled))
                {
                    if (enabled)
                    {
                        _configuration.EnabledActions!.Add(record.name);
                    }
                    else
                    {
                        _configuration.EnabledActions!.Remove(record.name);
                    }

                    SaveConfiguration();
                }

                ImGui.SameLine();
                ImGui.Image(_iconManager.GetRecordIcon(kv.Key).ImGuiHandle, new Vector2(45, 45));

                ImGui.SameLine();
                ImGui.Text(record.name);

                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudGrey2, record.tooltip);

                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudGrey2, $"{record.jobAffinity}, lvl.{record.minLvl}");
            }
        }

        private IEnumerable<KeyValuePair<ActionLibraryRecord, bool>> EnumerateActionsInCategory(ActionCategory category)
        {
            foreach (var record in _actionLibrary.AllRecords().Where(a => a.category == category))
            {
                var enabled = _configuration.EnabledActions!.Contains(record.name);

                yield return new KeyValuePair<ActionLibraryRecord, bool>(record, enabled);
            }
        }

        private void ImGuiHelpTooltip(string tooltip)
        {
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.8f, 1f), "?");
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(tooltip);
            }
        }

        private void SaveConfiguration()
        {
            _pluginInterface.SavePluginConfig(_configuration);
        }
    }
}