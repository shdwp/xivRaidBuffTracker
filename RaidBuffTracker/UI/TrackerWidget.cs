using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Colors;
using ImGuiNET;
using ImGuiScene;
using RaidBuffTracker.Tracker;
using RaidBuffTracker.Tracker.Library;

namespace RaidBuffTracker.UI
{
    public sealed class TrackerWidget : IDisposable
    {
        private readonly IStatusTracker _tracker;
        private readonly StatusLibrary  _statusLibrary;
        private readonly IconManager    _iconManager;

        public TrackerWidget(IStatusTracker tracker, StatusLibrary statusLibrary, IconManager iconManager)
        {
            _tracker = tracker;
            _statusLibrary = statusLibrary;
            _iconManager = iconManager;
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Test"))
            {
                var size = new Vector2(250, 250);
                var position = Vector2.Zero;

                foreach (var track in _tracker.EnumerateTracks())
                {
                    var icon = _iconManager.GetIcon(track.record);
                    var iconTint = ImGuiColors.DalamudWhite;
                    var cooldownText = "";

                    if (track.IsActive)
                    {
                        cooldownText = "" + (int)track.DurationRemaining;
                        iconTint = ImGuiColors.DalamudRed;
                    }
                    else if (track.IsReady)
                    {
                        cooldownText = "" + 0;
                        iconTint = ImGuiColors.DalamudWhite;
                    }
                    else
                    {
                        cooldownText = "" + (int)track.CooldownRemaining;
                        iconTint = ImGuiColors.DalamudGrey3;
                    }

                    ImGui.SetCursorPos(position);
                    ImGui.Image(icon.ImGuiHandle, size, Vector2.Zero, Vector2.One, iconTint);

                    ImGui.SetCursorPos(position + size / 2);
                    ImGui.Text(cooldownText);

                    if (position.X + size.X * 2 > ImGui.GetWindowWidth())
                    {
                        position.X = 0;
                        position.Y += size.Y;
                    }
                    else
                    {
                        position.X += size.X;
                    }
                    /*
                    ImGui.Text($"{track.source.name} - {track.record.id}");
                    ImGui.SameLine();
                    if (track.IsActive)
                    {
                        ImGui.Text($"Active ({track.DurationRemaining}");
                    }
                    else if (track.IsReady)
                    {
                        ImGui.Text($"Ready");
                    }
                    else
                    {
                        ImGui.Text($"CD {track.CooldownRemaining}");
                    }
                    */
                }

                ImGui.End();
            }
        }
    }
}