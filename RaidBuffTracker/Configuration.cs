using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Configuration;

namespace RaidBuffTracker
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        public Vector2 WidgetPosition            { get; set; } = new(250, 250);
        public Vector2 WidgetSize                { get; set; } = new(600, 300);
        public Vector2 WidgetCellSize            { get; set; } = new(100, 100);
        public bool    WidgetSplitIntoCategories { get; set; } = false;
        public bool    WidgetReverseOrder        { get; set; } = false;
        public bool    WidgetInteraction         { get; set; } = true;
        public bool    WidgetIcons               { get; set; } = false;
        public bool    OnlyInInstances           { get; set; }

        public List<string>? EnabledActions  { get; set; }
    }
}