using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace RaidBuffTracker.UI
{
    public sealed class ConfigurationWindow : Window
    {
        public ConfigurationWindow(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
        {
        }

        public override void Draw()
        {
        }
    }
}