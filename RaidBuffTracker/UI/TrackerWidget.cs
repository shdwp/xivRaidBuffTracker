using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Colors;
using ImGuiNET;
using RaidBuffTracker.Tracker;
using RaidBuffTracker.Tracker.Library;

namespace RaidBuffTracker.UI
{
    public sealed class TrackerWidget : IDisposable
    {
        public  bool    isLocked = true;

        private Vector2 _minSize = new(170, 200);

        private readonly ActionLibrary _actionLibrary;
        private readonly IconManager   _iconManager;
        private readonly Configuration _configuration;
        private readonly Condition     _condition;

        private IActionTracker? _tracker;

        private Vector4 _activeColor = new(0.3f, 1f, 0.3f, 1f);
        private Vector4 _activeFrame = new(0.3f, 1f, 0.3f, 1f);

        private Vector4 _readyColor = new(0.8f, 0.8f, 0.8f, 1f);
        private Vector4 _readyFrame = new(0.8f, 0.8f, 0.8f, 1f);

        private Vector4 _cooldownColor = new(0.3f, 0.3f, 0.3f, 1f);
        private Vector4 _cooldownFrame = new(0.3f, 0.3f, 0.3f, 1f);

        private Vector4 _shadowColor  = new(0f, 0f, 0f, 1f);
        private float   _shadowOffset = 10f;

        private Vector2 _cellPadding = new(15, 25);
        private Vector2 _jobIconSize = new(40, 40);

        private ImGuiWindowFlags _unlockedFlags = ImGuiWindowFlags.None;
        private ImGuiWindowFlags _lockedFlags   = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBackground;

        public TrackerWidget(ActionLibrary actionLibrary, IconManager iconManager, Condition condition, Configuration configuration)
        {
            _actionLibrary = actionLibrary;
            _iconManager = iconManager;
            _condition = condition;
            _configuration = configuration;
        }

        public void SetTracker(IActionTracker tracker)
        {
            _tracker = tracker;
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            if (_configuration.OnlyInInstances && !_condition[ConditionFlag.BoundByDuty])
            {
                return;
            }

            if (_tracker?.AnyTracks() != true)
            {
                return;
            }

            var currentPosition = new Vector2(_cellPadding.X, _cellPadding.Y + 25);

            var lockedFlags = _lockedFlags;
            if (!_configuration.WidgetInteraction)
            {
                lockedFlags |= ImGuiWindowFlags.NoMouseInputs;
            }

            ImGui.SetNextWindowSize(_configuration.WidgetPosition);
            ImGui.SetNextWindowSize(_configuration.WidgetSize);
            ImGui.SetNextWindowSizeConstraints(_minSize, new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("RaidBuffTracker widget", isLocked ? lockedFlags : _unlockedFlags))
            {
                var cellSize = _configuration.WidgetCellSize;

                var sortedTracks = _tracker.EnumerateTracks().ToList();
                sortedTracks.Sort((a, b) =>
                {
                    return a.record.category.CompareTo(b.record.category);
                });

                ActionCategory? lastCategory = null;
                if (_configuration.WidgetReverseOrder)
                {
                    sortedTracks.Reverse();
                    lastCategory = ActionCategory.Utility;
                }

                foreach (var track in sortedTracks)
                {
                    if (_configuration.EnabledActions?.Contains(track.record.name) == false)
                    {
                        continue;
                    }

                    if (_configuration.WidgetSplitIntoCategories && lastCategory != null && lastCategory != track.record.category)
                    {
                        currentPosition.X = _cellPadding.X;
                        currentPosition.Y += cellSize.Y + _cellPadding.Y;
                    }

                    lastCategory = track.record.category;

                    var recordIcon = _iconManager.GetRecordIcon(track.record);
                    var jobIcon = _iconManager.GetJobIcon(track.source.jobId);

                    var recordIconTint = ImGuiColors.DalamudWhite;
                    var recordIconFrame = ImGuiColors.DalamudWhite;
                    var text = "";
                    var textColor = ImGuiColors.DalamudWhite;

                    if (track.IsActive)
                    {
                        text = "" + (int)Math.Round(track.DurationRemaining);
                        textColor = ImGuiColors.HealerGreen;
                        recordIconTint = _activeColor;
                        recordIconFrame = _activeFrame;
                    }
                    else if (track.IsReady)
                    {
                        text = "";
                        recordIconTint = _readyColor;
                        recordIconFrame = _readyFrame;
                    }
                    else
                    {
                        text = "" + (int)Math.Round(track.CooldownRemaining);
                        textColor = ImGuiColors.DalamudRed;
                        recordIconTint = _cooldownColor;
                        recordIconFrame = _cooldownFrame;
                    }

                    ImGui.SetCursorPos(currentPosition);
                    ImGui.Image(recordIcon.ImGuiHandle, cellSize, Vector2.Zero, Vector2.One, recordIconTint, recordIconFrame);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip(track.record.name + "\n" + track.source.name + " - " + track.record.category + "\n" + track.record.tooltip);
                    }


                    if (_configuration.WidgetIcons)
                    {

                        var indexString = (track.source.index - 1).ToString(); // indexes go up to 9 for some reason
                        var bottomPartTextSize = new Vector2(_jobIconSize.X - 15, _jobIconSize.Y - 10);
                        var bottomPartSize = new Vector2(_jobIconSize.X + bottomPartTextSize.X, _jobIconSize.Y);

                        ImGui.SetCursorPos(currentPosition + new Vector2(cellSize.X / 2 - bottomPartSize.X / 2, cellSize.Y - bottomPartSize.Y / 2));
                        ImGui.Image(jobIcon.ImGuiHandle, _jobIconSize);

                        var bottomPartTextPosition = currentPosition + new Vector2(
                                                         cellSize.X / 2 - bottomPartSize.X / 2 + _jobIconSize.X,
                                                         cellSize.Y - bottomPartSize.Y / 2 + (_jobIconSize.Y - bottomPartTextSize.Y) / 2);

                        ImGui.SetCursorPos(bottomPartTextPosition);
                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 1));
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0, 0, 0, 1));
                        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0, 0, 0, 1));
                        ImGui.Button("", bottomPartTextSize);
                        ImGui.PopStyleColor(3);

                        ImGui.SetWindowFontScale(1.5f);
                        ImGui.SetCursorPos(bottomPartTextPosition + new Vector2(6, 3));
                        ImGui.TextColored(new Vector4(1, 1, 1, 1), indexString);
                    }

                    ImGui.SetWindowFontScale(3f);

                    var textSize = ImGui.CalcTextSize(text);
                    ImGui.SetCursorPos(currentPosition + cellSize / 2 - textSize / 2);
                    DrawTextWithShadow(textColor, text, _shadowColor, _shadowOffset);

                    if (currentPosition.X + cellSize.X * 2 + _cellPadding.X * 2 > ImGui.GetWindowWidth())
                    {
                        currentPosition.X = _cellPadding.X;
                        currentPosition.Y += cellSize.Y + _cellPadding.Y;
                    }
                    else
                    {
                        currentPosition.X += cellSize.X + _cellPadding.X;
                    }
                }

                _configuration.WidgetPosition = ImGui.GetWindowPos();
                _configuration.WidgetSize = ImGui.GetWindowSize();
                ImGui.End();
            }
        }

        private void DrawTextWithShadow(Vector4 textColor, string text, Vector4 shadowColor, float shadowOffset)
        {
            var currentPosition = ImGui.GetCursorPos();

            for (var x = 0; x < shadowOffset; x += 3)
            {
                for (var y = 0; y < shadowOffset; y += 3)
                {
                    ImGui.SetCursorPos(currentPosition + new Vector2(x - shadowOffset / 2, y - shadowOffset / 2));
                    ImGui.TextColored(shadowColor, text);
                }
            }

            ImGui.SetCursorPos(currentPosition);
            ImGui.TextColored(textColor, text);
            ImGui.SetWindowFontScale(1f);
        }
    }
}
