using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Colors;
using ImGuiNET;
using ImGuiScene;
using RaidBuffTracker.Tracker;
using RaidBuffTracker.Tracker.Library;
using RaidBuffTracker.Tracker.Track;
using RaidBuffTracker.UI.Layout;
using RaidBuffTracker.Utils;

namespace RaidBuffTracker.UI;

public sealed class TrackerWidget : IDisposable
{
    public TrackerWidget(ActionLibrary actionLibrary, TextureManager textureManager, Condition condition, Configuration configuration, ClientState clientState)
    {
        _actionLibrary = actionLibrary;
        _textureManager = textureManager;
        _condition = condition;
        _configuration = configuration;
        _clientState = clientState;
        _layout = new HorizontalCategoryTableTrackerWidgetLayout();
    }

    public void SetTracker(IActionTracker tracker)
    {
        _tracker = tracker;
    }

    // Token: 0x0600007A RID: 122 RVA: 0x00003880 File Offset: 0x00001A80
    public void SetLayout(ITrackerWidgetLayout layout)
    {
        _layout = layout;
    }

    // Token: 0x0600007B RID: 123 RVA: 0x0000388A File Offset: 0x00001A8A
    public void Dispose()
    {
    }

    // Token: 0x0600007C RID: 124 RVA: 0x00003890 File Offset: 0x00001A90
    public void Draw()
    {
        Diag.Assert(_layout != null, "layout is null!");
        Diag.Assert(_tracker != null, "tracker is null!");
        bool flag = isLocked && _configuration.OnlyInInstances && !_condition[34];
        if (!flag)
        {
            bool flag2 = !_tracker.AnyTracks();
            if (!flag2)
            {
                ImGuiWindowFlags lockedFlags = _lockedFlags;
                bool flag3 = !_configuration.MouseInteraction;
                if (flag3)
                {
                    lockedFlags |= ImGuiWindowFlags.NoMouseInputs;
                }

                ImGui.SetNextWindowSize(_configuration.WidgetPosition);
                ImGui.SetNextWindowSize(_configuration.WidgetSize);
                ImGui.SetNextWindowSizeConstraints(_minSize, new Vector2(float.MaxValue, float.MaxValue));
                bool flag4 = ImGui.Begin("RaidBuffTracker unlocked widget", isLocked ? lockedFlags : _unlockedFlags);
                if (flag4)
                {
                    float globalScale = ImGui.GetIO().FontGlobalScale;
                    _cellSize = _configuration.WidgetCellSize * globalScale;
                    _categoryIconSize = new Vector2(20f, 26f) * globalScale;
                    Vector2 jobIconSize = Vector2.Max(_cellSize / 2.5f, new Vector2(16f, 16f) * globalScale);
                    Vector2 cellMargin = new Vector2(5f, 5f + jobIconSize.Y / globalScale / 2f) * globalScale;
                    ActionCategory? lastCategory = null;
                    _layout.StartOver(_cellSize, cellMargin, _categoryIconSize, ImGui.GetWindowSize());
                    foreach (ActionTrack track in GetDisplayedTrackList())
                    {
                        bool flag5 = _configuration.SplitIntoCategories ||
                                     _configuration.WidgetLayout == TrackerWidgetLayout.HorizontalCategoryTable ||
                                     _configuration.WidgetLayout == TrackerWidgetLayout.VerticalCategoryTable;
                        if (flag5)
                        {
                            bool flag6;
                            if (lastCategory != null)
                            {
                                ActionCategory? actionCategory = lastCategory;
                                ActionCategory category = track.record.category;
                                flag6 = !(actionCategory.GetValueOrDefault() == category & actionCategory != null);
                            }
                            else
                            {
                                flag6 = false;
                            }

                            bool flag7 = flag6;
                            if (flag7)
                            {
                                _layout.AdvanceToNextSection();
                            }

                            bool flag8;
                            if (lastCategory != null)
                            {
                                ActionCategory? actionCategory = lastCategory;
                                ActionCategory category = track.record.category;
                                flag8 = !(actionCategory.GetValueOrDefault() == category & actionCategory != null);
                            }
                            else
                            {
                                flag8 = true;
                            }

                            bool flag9 = flag8;
                            if (flag9)
                            {
                                DrawCategoryIcon(track.record.category);
                                _layout.AdvancePastHeader();
                            }

                            lastCategory = new ActionCategory?(track.record.category);
                        }

                        TextureWrap jobIcon = _textureManager.GetJobIcon(track.source.jobId);
                        Vector4 recordIconTint = ImGuiColors.DalamudWhite;
                        Vector4 recordIconFrame = ImGuiColors.DalamudWhite;
                        string text = "";
                        Vector4 textColor = ImGuiColors.DalamudWhite;
                        bool isActive = track.IsActive;
                        if (isActive)
                        {
                            bool showActiveCountdown = _configuration.ShowActiveCountdown;
                            if (showActiveCountdown)
                            {
                                text = (((int)Math.Round(track.DurationRemaining)).ToString() ?? "");
                                textColor = ImGuiColors.HealerGreen;
                            }

                            recordIconTint = _activeTintColor;
                        }
                        else
                        {
                            bool isReady = track.IsReady;
                            if (isReady)
                            {
                                text = "";
                                textColor = ImGuiColors.HealerGreen;
                                recordIconTint = _readyTintColor;
                            }
                            else
                            {
                                text = (((int)Math.Round(track.CooldownRemaining)).ToString() ?? "");
                                textColor = ImGuiColors.DalamudWhite;
                                recordIconTint = _cooldownTintColor;
                            }
                        }

                        DrawActionIcon(track, recordIconTint);
                        bool flag10 = track.IsActive || track.IsReady;
                        if (flag10)
                        {
                            DrawActionOverlay();
                        }
                        else
                        {
                            DrawActionCooldownOverlay(track);
                        }

                        bool flag11 = ImGui.IsItemHovered();
                        if (flag11)
                        {
                            string categoryName = UICommon.GetActionCategoryName(track.record.category);
                            ImGui.SetTooltip(string.Concat(new string[]
                            {
                                track.record.name,
                                "\n",
                                track.source.name,
                                " - ",
                                categoryName,
                                "\n",
                                track.record.tooltip
                            }));
                        }

                        bool bottomIconsEnabled = _configuration.BottomIconsEnabled;
                        if (bottomIconsEnabled)
                        {
                            string indexString = track.source.index.ToString();
                            Vector2 bottomPartTextSize = new Vector2(jobIconSize.X - 15f, jobIconSize.Y - 10f);
                            Vector2 bottomPartSize = new Vector2(jobIconSize.X + bottomPartTextSize.X, jobIconSize.Y);
                            ImGui.SetCursorPos(_layout.GetPos(new Vector2(
                                _cellSize.X / 2f - bottomPartSize.X / 2f,
                                _cellSize.Y - bottomPartSize.Y / 2f)));
                            ImGui.Image(jobIcon.ImGuiHandle, jobIconSize);
                            Vector2 bottomPartTextPosition = _layout.GetPos(
                                new Vector2(_cellSize.X / 2f - bottomPartSize.X / 2f + jobIconSize.X,
                                    _cellSize.Y - bottomPartSize.Y / 2f +
                                    (jobIconSize.Y - bottomPartTextSize.Y) / 2f));
                            ImGui.SetCursorPos(bottomPartTextPosition);
                            ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DalamudWhite);
                            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGuiColors.DalamudWhite);
                            ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGuiColors.DalamudWhite);
                            ImGui.Button("", bottomPartTextSize);
                            ImGui.PopStyleColor(3);
                            Vector2 indexSize = ImGui.CalcTextSize(indexString);
                            ImGui.SetCursorPos(bottomPartTextPosition + bottomPartTextSize / 2f - indexSize / 2f -
                                               new Vector2(0f, 1f * globalScale));
                            ImGui.TextColored(new Vector4(0f, 0f, 0f, 1f), indexString);
                        }

                        ImGui.SetWindowFontScale(_cellSize.X * 0.032f / globalScale);
                        Vector2 textSize = ImGui.CalcTextSize(text);
                        ImGui.SetCursorPos(_layout.GetPos(_cellSize / 2f - textSize / 2f));
                        DrawTextWithShadow(textColor, text, _shadowColor, _shadowOffset);
                        ImGui.SetWindowFontScale(1f);
                        _layout.AdvanceToNextCell();
                    }

                    _configuration.WidgetPosition = ImGui.GetWindowPos();
                    _configuration.WidgetSize = ImGui.GetWindowSize();
                    ImGui.End();
                }
            }
        }
    }

    // Token: 0x0600007D RID: 125 RVA: 0x00003FC8 File Offset: 0x000021C8
    private List<ActionTrack> GetDisplayedTrackList()
    {
        List<ActionTrack> sortedTracks = _tracker.EnumerateTracks().Where(delegate(ActionTrack t)
        {
            List<string> enabledActions = _configuration.EnabledActions;
            bool flag = enabledActions == null || !enabledActions.Contains(t.record.name);
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                uint objectId = t.source.objectId;
                PlayerCharacter localPlayer = _clientState.LocalPlayer;
                uint? num = (localPlayer != null) ? new uint?(localPlayer.ObjectId) : null;
                bool flag2 = (objectId == num.GetValueOrDefault() & num != null) &&
                             !_configuration.ShowPlayerActions;
                result = !flag2;
            }

            return result;
        }).ToList<ActionTrack>();
        sortedTracks.Sort((ActionTrack a, ActionTrack b) =>
            string.Compare(a.record.name, b.record.name, StringComparison.CurrentCulture));
        sortedTracks.Sort((ActionTrack a, ActionTrack b) => a.record.category.CompareTo(b.record.category));
        return sortedTracks;
    }

    // Token: 0x0600007E RID: 126 RVA: 0x0000404C File Offset: 0x0000224C
    private void DrawCategoryIcon(ActionCategory category)
    {
        ImGui.SetCursorPos(_layout.GetPos(new Vector2(_cellSize.X / 2f - _categoryIconSize.X / 2f,
            _cellSize.Y / 2f - _categoryIconSize.Y / 2f)));
        ImGui.Image(_textureManager.GetCategoryIcon(category).ImGuiHandle, _categoryIconSize);
    }

    // Token: 0x0600007F RID: 127 RVA: 0x000040D4 File Offset: 0x000022D4
    private void DrawActionIcon(ActionTrack track, Vector4 tint)
    {
        TextureWrap recordIcon = _textureManager.GetRecordIcon(track.record);
        Vector2 size = new Vector2(_cellSize.X - _cellSize.X * 0.075f,
            _cellSize.Y - _cellSize.Y * 0.075f);
        float offset = size.X * 0.042f;
        Vector2 pos = _layout.GetPos(new Vector2(offset, 0f));
        ImGui.SetCursorPos(pos);
        ImGui.Image(recordIcon.ImGuiHandle, size, Vector2.Zero, Vector2.One, tint);
    }

    // Token: 0x06000080 RID: 128 RVA: 0x00004178 File Offset: 0x00002378
    private void DrawActionOverlay()
    {
        ImGui.SetCursorPos(_layout.GetPos(default(Vector2)));
        Vector2 uv0 = new Vector2(0.0046948357f, 0.013888889f);
        Vector2 uv = new Vector2(0.10798122f, 0.33333334f);
        ImGui.Image(_textureManager.actionFrame.ImGuiHandle, _cellSize, uv0, uv);
    }

    // Token: 0x06000081 RID: 129 RVA: 0x000041E0 File Offset: 0x000023E0
    private void DrawActionCooldownOverlay(ActionTrack track)
    {
        int uvCellCount = 81;
        Vector2 uvCellSize = new Vector2(0.11111111f, 0.11111111f);
        double cooldownFactor = 1.0 - track.CooldownRemaining / track.record.cooldown;
        int uvCellIndex = (int)((double)uvCellCount * cooldownFactor);
        int uvCol = uvCellIndex / 9;
        int uvRow = uvCellIndex % 9;
        Vector2 uv = new Vector2((float)uvRow * uvCellSize.X, (float)uvCol * uvCellSize.Y);
        ImGui.SetCursorPos(_layout.GetPos(default(Vector2)));
        ImGui.Image(_textureManager.actionCooldownSheet.ImGuiHandle, _cellSize, uv,
            uv + uvCellSize - new Vector2(0f, 0.005787037f));
    }

    // Token: 0x06000082 RID: 130 RVA: 0x000042A4 File Offset: 0x000024A4
    private void DrawTextWithShadow(Vector4 textColor, string text, Vector4 shadowColor, float shadowOffset)
    {
        Vector2 currentPosition = ImGui.GetCursorPos();
        int x = 0;
        while ((float)x < shadowOffset)
        {
            int y = 0;
            while ((float)y < shadowOffset)
            {
                ImGui.SetCursorPos(currentPosition +
                                   new Vector2((float)x - shadowOffset / 2f, (float)y - shadowOffset / 2f));
                ImGui.TextColored(shadowColor, text);
                y += 2;
            }

            x += 2;
        }

        ImGui.SetCursorPos(currentPosition);
        ImGui.TextColored(textColor, text);
        ImGui.SetWindowFontScale(1f);
    }

    // Token: 0x0400008F RID: 143
    public bool isLocked = true;

    // Token: 0x04000090 RID: 144
    private Vector2 _minSize = new Vector2(170f, 200f);

    // Token: 0x04000091 RID: 145
    private readonly ActionLibrary _actionLibrary;

    // Token: 0x04000092 RID: 146
    private readonly TextureManager _textureManager;

    // Token: 0x04000093 RID: 147
    private readonly Configuration _configuration;

    // Token: 0x04000094 RID: 148
    private readonly Condition _condition;

    // Token: 0x04000095 RID: 149
    private readonly ClientState _clientState;

    // Token: 0x04000096 RID: 150
    private IActionTracker _tracker;

    // Token: 0x04000097 RID: 151
    private ITrackerWidgetLayout _layout;

    // Token: 0x04000098 RID: 152
    private Vector4 _activeTintColor = new Vector4(0.5f, 0.8f, 0.5f, 1f);

    // Token: 0x04000099 RID: 153
    private Vector4 _readyTintColor = new Vector4(1f, 1f, 1f, 1f);

    // Token: 0x0400009A RID: 154
    private Vector4 _cooldownTintColor = new Vector4(1f, 1f, 1f, 1f);

    // Token: 0x0400009B RID: 155
    private Vector4 _shadowColor = new Vector4(0f, 0f, 0f, 1f);

    // Token: 0x0400009C RID: 156
    private float _shadowOffset = 8f;

    // Token: 0x0400009D RID: 157
    private Vector2 _cellSize;

    // Token: 0x0400009E RID: 158
    private Vector2 _categoryIconSize;

    // Token: 0x0400009F RID: 159
    private ImGuiWindowFlags _unlockedFlags = 0;

    // Token: 0x040000A0 RID: 160
    private ImGuiWindowFlags _lockedFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize;
}