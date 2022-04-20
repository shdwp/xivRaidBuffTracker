using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Data;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Party;
using Dalamud.Logging;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using RaidBuffTracker.Tracker.Track;
using RaidBuffTracker.Utils;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace RaidBuffTracker.Tracker.Library
{
    public sealed class ActionLibrary
    {
        private DataManager _dataManager;

        private ExcelSheet<Action>   _actionSheet;
        private ExcelSheet<ClassJob> _jobSheet;

        private readonly List<ActionLibraryRecord> _db = new();

        public ActionLibrary(DataManager dataManager)
        {
            _dataManager = dataManager;
            _actionSheet = dataManager.GameData.GetExcelSheet<Action>(Language.English);
            _jobSheet = dataManager.GameData.GetExcelSheet<ClassJob>();

            Diag.Assert(_actionSheet != null && _jobSheet != null, "Status library sheet retrieval");
            RegisterAll();
        }

        public ActionLibraryRecord FindRecord(string name)
        {
            return _db.FirstOrDefault(kv => kv.name == name);
        }

        public IReadOnlyCollection<ActionLibraryRecord> AllRecords()
        {
            return _db;
        }

        public ActionTrack InstantiateTrack(PlayerCharacter playerCharacter, ActionLibraryRecord @record)
        {
            return new ActionTrack(playerCharacter.ToActionTrackSource(1), record);
        }

        public ActionTrack InstantiateTrack(ActionTrackSource source, ActionLibraryRecord @record)
        {
            return new ActionTrack(source, record);
        }

        public IEnumerable<ActionTrack> InstantiatePartyTracks(IEnumerable<PartyMember> members)
        {
            foreach (var record in _db)
            {
                uint index = 1;
                foreach (var member in members)
                {
                    var source = member.ToActionTrackSource(index);
                    if (record.IsApplicableToSource(source))
                    {
                        yield return new ActionTrack(source, record);
                    }

                    index++;
                }
            }
        }

        public IEnumerable<ActionTrack> InstantiatePlayerTracks(PlayerCharacter playerCharacter)
        {
            foreach (var record in _db)
            {
                var source = playerCharacter.ToActionTrackSource(2);
                if (record.IsApplicableToSource(source))
                {
                    yield return new ActionTrack(source, record);
                }
            }
        }

        private void RegisterAll()
        {
            Diag.MeasureTime("Library load", () =>
            {
                // Role
                Register(ActionCategory.MitigationPrimary, "Reprisal", 10, "-10%% incoming damage");
                Register(ActionCategory.Utility, "Head Graze", 0f, "casting interrupt");
                Register(ActionCategory.Utility, "Interject", 0f, "casting interrupt");
                Register(ActionCategory.Utility, "Addle", 10f, "-10%% int & mind");
                Register(ActionCategory.Utility, "Feint", 10f, "-10%% str & dex");
                Register(ActionCategory.Utility, "Rescue", 0f, "pull party member");
                Register(ActionCategory.Utility, "Swiftcast", 0f, "swiftcast");

                // PLD
                Register(ActionCategory.MitigationPrimary, "Divine Veil", 30, "10%% max hp shield");

                // WAR
                Register(ActionCategory.MitigationPrimary, "Shake It Off", 15, "12%% max hp shield");

                // DRK
                Register(ActionCategory.MitigationSecondary, "Dark Missionary", 15, "-10%% magical damage");

                // GNB
                Register(ActionCategory.MitigationSecondary, "Heart of Light", 15, "-10%% magical damage");

                // REP
                Register(ActionCategory.OffensivePrimary, "Arcane Circle", 20f, "+3%% damage");
                Register(ActionCategory.MitigationPrimary, "Arcane Crest", 5, "10%% max HP shield");

                // AST
                Register(ActionCategory.OffensivePrimary, "Divination", 15f, "+4%%-6%% damage");
                Register(ActionCategory.MitigationPrimary, "Collective Unconscious", 20, "-10%% incoming damage");
                Register(ActionCategory.OffensiveSecondary, "Draw", 0f, "no hint");

                // NIN
                Register(ActionCategory.OffensivePrimary, "Mug", 20f, "+5%% damage");

                // DRG
                Register(ActionCategory.OffensivePrimary, "Battle Litany", 20f, "+10%% crit");
                Register(ActionCategory.OffensiveSecondary, "Dragon Sight", 20f, "+5%% damage for partner");

                // MNK
                Register(ActionCategory.OffensivePrimary, "Brotherhood", 15f, "+5%% phys. damage");
                Register(ActionCategory.Utility, "Mantra", 15f, "+10%% healing");

                // BRD
                Register(ActionCategory.OffensivePrimary, "Battle Voice", 15f, "+20%% dir. hit rate");
                Register(ActionCategory.OffensiveSecondary, "Radiant Finale", 15f, "+2/4/6%% damage");
                Register(ActionCategory.OffensiveSecondary, "the Wanderer's Minuet", 45f, "+2%% crit");
                Register(ActionCategory.OffensiveSecondary, "Mage's Ballad", 45f, "+1%% damage");
                Register(ActionCategory.OffensiveSecondary, "Army's Paeon", 45f, "+3%% dir. hit rate");
                Register(ActionCategory.MitigationPrimary, "Troubadour", 20, "-10%% incoming damage");

                // RDM
                Register(ActionCategory.OffensivePrimary, "Embolden", 20f, "+10%% phys. damage (decaying)");
                Register(ActionCategory.MitigationSecondary, "Magick Barrier", 10, "-10%% magic dmg, +5%% healing");

                // MCH
                Register(ActionCategory.MitigationPrimary, "Tactician", 15, "-10%% incoming damage");

                // DNC
                Register(ActionCategory.OffensivePrimary, "Technical Finish", new[] { "Technical Step" }, new[] { "Single Technical Finish", "Double Technical Finish", "Triple Technical Finish", "Quadruple Technical Finish" }, 20f, "+5%% damage");
                Register(ActionCategory.OffensiveSecondary, "Devilment", 20f, "+20%% crit and direct hit for partner");
                Register(ActionCategory.OffensiveSecondary, "Standard Finish", new[] { "Standard Step" }, new[] { "Single Standard Finish", "Double Standard Finish" }, 90f, "no hint");
                Register(ActionCategory.MitigationPrimary, "Shield Samba", 15, "-10%% incoming damage");

                // SMN
                Register(ActionCategory.OffensivePrimary, "Searing Light", 30f, "+3%% damage");

                // SCH
                Register(ActionCategory.OffensivePrimary, "Chain Stratagem", 15f, "+10%% crit");
                Register(ActionCategory.MitigationSecondary, "Fey Illumination", 20, "-5%% magical damage, +10%% healing");

                // WHM
                Register(ActionCategory.MitigationPrimary, "Temperance", 20, "-10%% incoming damage");
            });
        }

        private void Register(ActionCategory category, string actionName, float duration, string tooltip)
        {
            Register(category, null, new[] { actionName }, new[] { actionName }, duration, tooltip);
        }

        private void Register(ActionCategory category, string? name, string[] cooldownActionNames, string[] effectActionNames, float duration, string tooltip)
        {
            Diag.Assert(cooldownActionNames.Any() && effectActionNames.Any(), "No actions passed for registration");

            var cooldownActionIds = new List<uint>();
            var effectActionIds = new List<uint>();
            Action titleAction = null;
            Action cooldownAction = null;

            foreach (var cooldownActionName in cooldownActionNames)
            {
                var action = FindAction(cooldownActionName);
                if (action == null)
                {
                    PluginLog.Warning($"Failed to register - couldn't find actions {cooldownActionName}");
                    return;
                }

                cooldownActionIds.Add(action.RowId);
                cooldownAction ??= action;
            }

            foreach (var effectActionName in effectActionNames)
            {
                var action = FindAction(effectActionName);
                if (action == null)
                {
                    PluginLog.Warning($"Failed to register - couldn't find actions {effectActionName}");
                    return;
                }

                effectActionIds.Add(action.RowId);
                titleAction ??= action;
            }

            var jobAffinity = titleAction.ClassJobCategory.Value.Name.ToString();
            var cooldown = cooldownAction.Recast100ms / 10f;

            PluginLog.Debug($"Registering {titleAction.ClassJob.Value?.Abbreviation} ({titleAction.ClassJob.Row}) {titleAction.Name} - jobs {jobAffinity} level {titleAction.ClassJobLevel} cooldown {cooldown} ");
            var record = new ActionLibraryRecord
            {
                name = name ?? titleAction.Name.ToString(),
                category = category,
                iconId = titleAction.Icon,
                tooltip = tooltip,

                jobAffinity = jobAffinity,
                minLvl = titleAction.ClassJobLevel,

                cooldownActionIds = cooldownActionIds.ToArray(),
                effectActionIds = effectActionIds.ToArray(),
                duration = duration,
                cooldown = cooldown,
            };

            _db.Add(record);
        }

        private Action? FindAction(string actionName)
        {
            var action = _actionSheet.FirstOrDefault(a => a.Name.ToString() == actionName && a.ClassJobLevel > 0);
            if (action == null)
            {
                return null;
            }

            return action;
        }
    }
}