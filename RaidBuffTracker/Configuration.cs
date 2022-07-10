using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Configuration;
using RaidBuffTracker.UI.Layout;

namespace RaidBuffTracker
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        public Vector2 WidgetPosition { get; set; } = new(250, 250);
        public Vector2 WidgetSize { get; set; } = new(600, 300);

        public Vector2 WidgetCellSize { get; set; } = new(100, 100);

        public string WidgetLayout { get; set; } = TrackerWidgetLayout.Horizontal;

        public bool MouseInteraction { get; set; } = true;

        public bool BottomIconsEnabled { get; set; } = true;

        public bool ShowActiveCountdown { get; set; } = true;

        public bool ShowPlayerActions { get; set; } = false;

        public bool SplitIntoCategories { get; set; } = false;

        public bool OnlyInInstances { get; set; }
        public List<string>? EnabledActions { get; set; }
    }
}