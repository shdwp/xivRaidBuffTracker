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

namespace RaidBuffTracker.Tracker.Library
{
    public sealed class ActionLibrary
    {
        private DataManager _dataManager;

        private ExcelSheet<Action> _actionSheet;
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

        public ActionTrack InstantiateTrack(PlayerCharacter playerCharacter, ActionLibraryRecord record)
        {
            return new ActionTrack(playerCharacter.ToActionTrackSource(1), record);
        }

        public ActionTrack InstantiateTrack(ActionTrackSource source, ActionLibraryRecord record)
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
                var source = playerCharacter.ToActionTrackSource(1);
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
                Register(ActionCategory.MitigationPrimary, "Reprisal", 10f, "-10%% Incoming damage");
                Register(ActionCategory.Utility, "Head Graze", 0f, "Casting interrupt");
                Register(ActionCategory.Utility, "Interject", 0f, "Casting interrupt");
                Register(ActionCategory.MitigationSecondary, "Addle", 10f, "-10%% INT & MND");
                Register(ActionCategory.MitigationSecondary, "Feint", 10f, "-10%% STR & DEX");
                Register(ActionCategory.Utility, "Rescue", 0f, "Pull party member");
                Register(ActionCategory.Utility, "Swiftcast", 0f, "Makes next cast instant");
                Register(ActionCategory.MitigationPrimary, "Divine Veil", 30f,
                    "Party shield of 10%% of max PLD HP, heal for 400 potency");
                Register(ActionCategory.MitigationPrimary, "Hallowed Ground", 10f, "Invulnerability");
                Register(ActionCategory.MitigationSecondary, "Passage of Arms", 18f,
                    "100%% block rate, party -15%% incoming damage");
                Register(ActionCategory.MitigationPrimary, "Shake It Off", 15f,
                    "Party shield of 12%% of their max HP");
                Register(ActionCategory.MitigationPrimary, "Holmgang", 10f, "HP cannot go below 1");
                Register(ActionCategory.MitigationPrimary, "Dark Missionary", 15f,
                    "Team suffers -10%% magical damage taken");
                Register(ActionCategory.MitigationPrimary, "Living Dead", 10f,
                    "Once user dies, HP cannot go below 1. Must be healed up after");
                Register(ActionCategory.MitigationPrimary, "Heart of Light", 15f,
                    "Team suffers -10%% magical damage taken");
                Register(ActionCategory.MitigationPrimary, "Superbolide", 10f, "Invulnerable. HP reduced to 1");
                Register(ActionCategory.OffensivePrimary, "Arcane Circle", 20f, "Team deals +3%% more damage");
                Register(ActionCategory.MitigationPrimary, "Arcane Crest", 5f,
                    "Party healing over team when shield is broken");
                Register(ActionCategory.MitigationPrimary, "Collective Unconscious", 20f,
                    "Party -10%% incoming damage");
                Register(ActionCategory.OffensivePrimary, "Trick Attack", 15f, "Target +5%% incoming damage");
                Register(ActionCategory.OffensivePrimary, "Battle Litany", 20f, "Party +10%% critical hit rate");
                Register(ActionCategory.OffensiveSecondary, "Dragon Sight", 20f, "+5%% damage to partner");
                Register(ActionCategory.OffensivePrimary, "Brotherhood", 15f, "Party +5%% damage");
                Register(ActionCategory.Utility, "Mantra", 15f, "Party +10%% more GCD healing");
                Register(ActionCategory.OffensivePrimary, "Battle Voice", 15f, "Party +20%% Direct Hit rate");
                Register(ActionCategory.OffensiveSecondary, "Radiant Finale", 15f, "Party +2/4/6%% damage");
                Register(ActionCategory.OffensiveSecondary, "the Wanderer's Minuet", 45f,
                    "Party +2%% Critical Hit rate");
                Register(ActionCategory.OffensiveSecondary, "Mage's Ballad", 45f, "Party +1%% damage");
                Register(ActionCategory.OffensiveSecondary, "Army's Paeon", 45f, "Party +3%% Direct Hit rate");
                Register(ActionCategory.MitigationPrimary, "Troubadour", 15f, "Party -10%% Incoming damage");
                Register(ActionCategory.OffensivePrimary, "Embolden", 20f, "Party +5%% damage");
                Register(ActionCategory.MitigationSecondary, "Magick Barrier", 10f,
                    "Party -10%% Incoming damage, +5%% GCD healing");
                Register(ActionCategory.MitigationPrimary, "Tactician", 15f,
                    "Party -10%% Magical damage, +5%% GCD healing");
                Register(ActionCategory.OffensivePrimary, "Technical Finish", new[]
                {
                    "Technical Step"
                }, new[]
                {
                    "Single Technical Finish",
                    "Double Technical Finish",
                    "Triple Technical Finish",
                    "Quadruple Technical Finish"
                }, 20f, "Party +1/2/3/5%% damage");
                Register(ActionCategory.OffensiveSecondary, "Devilment", 20f,
                    "Dance Partner +20%% Critical Hit rate and Direct Hit rate");
                Register(ActionCategory.OffensiveSecondary, "Standard Finish", new[]
                {
                    "Standard Step"
                }, new[]
                {
                    "Single Standard Finish",
                    "Double Standard Finish"
                }, 15f, "Dance Partner +2/5%% damage");
                Register(ActionCategory.MitigationSecondary, "Shield Samba", 15f, "Party -10%% Incoming damage.");
                Register(ActionCategory.MitigationSecondary, "Improvisation", 15f, "Healing over time");
                Register(ActionCategory.OffensivePrimary, "Searing Light", 30f, "Party +3%% damage");
                Register(ActionCategory.OffensivePrimary, "Divination", 15f, "Party +6%% damage");
                Register(ActionCategory.OffensivePrimary, "Chain Stratagem", 15f, "Party +10%% Critical Hit rate");
                Register(ActionCategory.MitigationPrimary, "Fey Illumination", 20f,
                    "Party -5%% incoming Magical damage, +10%% GCD healing");
                Register(ActionCategory.MitigationSecondary, "Expedient", 15f,
                    "Party -10%% damage taken, movement speed increase");
                Register(ActionCategory.MitigationSecondary, "Sacred Soil", 15f, "Party -10%% damage taken");
                Register(ActionCategory.MitigationSecondary, "Dissipation", 30f, "User +20%% GCD healing");
                Register(ActionCategory.MitigationPrimary, "Temperance", 20f,
                    "Party -10%% damage and users GCD healing increased by 20%%");
                Register(ActionCategory.MitigationPrimary, "Kerachole", 15f, "Party -10%% Incoming damage");
                Register(ActionCategory.MitigationPrimary, "Holos", 20f, "Party -10%% Incoming damage");
            });
        }

        private void Register(ActionCategory category, string actionName, float duration, string tooltip)
        {
            Register(category, null, new[] { actionName }, new[] { actionName }, duration, tooltip);
        }

        private void Register(ActionCategory category, string? name, string[] cooldownActionNames,
            string[] effectActionNames, float duration, string tooltip)
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

            PluginLog.Debug(
                $"Registering {titleAction.ClassJob.Value?.Abbreviation} ({titleAction.ClassJob.Row}) {titleAction.Name} - jobs {jobAffinity} level {titleAction.ClassJobLevel} cooldown {cooldown} ");
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