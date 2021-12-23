using System;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Buddy;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.PartyFinder;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Libc;
using Dalamud.Game.Network;
using Dalamud.IoC;
using Dalamud.Plugin;
using Ninject;
using Ninject.Modules;
using RaidBuffTracker.Tracker;
using RaidBuffTracker.Tracker.Impl;
using RaidBuffTracker.Tracker.Library;
using RaidBuffTracker.Tracker.Source;
using RaidBuffTracker.UI;
using RaidBuffTracker.Utils;

namespace RaidBuffTracker
{
    public sealed class Module : NinjectModule
    {
 #pragma warning disable 8618
        public static IReadOnlyKernel shared;
 #pragma warning restore 8618

        private readonly object _scope = new();

        [PluginService] private DalamudPluginInterface PluginInterface { get; set; }
        [PluginService] private BuddyList              BuddyList       { get; set; }
        [PluginService] private ChatGui                ChatGui         { get; set; }
        [PluginService] private ChatHandlers           ChatHandlers    { get; set; }
        [PluginService] private ClientState            ClientState     { get; set; }
        [PluginService] private CommandManager         CommandManager  { get; set; }
        [PluginService] private Condition              Condition       { get; set; }
        [PluginService] private DataManager            DataManager     { get; set; }
        [PluginService] private FateTable              FateTable       { get; set; }
        [PluginService] private FlyTextGui             FlyTextGui      { get; set; }
        [PluginService] private Framework              Framework       { get; set; }
        [PluginService] private GameGui                GameGui         { get; set; }
        [PluginService] private GameNetwork            GameNetwork     { get; set; }
        [PluginService] private JobGauges              JobGauges       { get; set; }
        [PluginService] private KeyState               KeyState        { get; set; }
        [PluginService] private LibcFunction           LibcFunction    { get; set; }
        [PluginService] private ObjectTable            ObjectTable     { get; set; }
        [PluginService] private PartyFinderGui         PartyFinderGui  { get; set; }
        [PluginService] private PartyList              PartyList       { get; set; }
        [PluginService] private SigScanner             SigScanner      { get; set; }
        [PluginService] private TargetManager          TargetManager   { get; set; }
        [PluginService] private ToastGui               ToastGui        { get; set; }

 #pragma warning disable 8618
        public Module(DalamudPluginInterface pi)
 #pragma warning restore 8618
        {
            pi.Inject(this);
        }

        public override void Load()
        {
            BindDalamudService(PluginInterface);
            BindDalamudService(BuddyList);
            BindDalamudService(ChatGui);
            BindDalamudService(ChatHandlers);
            BindDalamudService(ClientState);
            BindDalamudService(CommandManager);
            BindDalamudService(Condition);
            BindDalamudService(DataManager);
            BindDalamudService(FateTable);
            BindDalamudService(FlyTextGui);
            BindDalamudService(Framework);
            BindDalamudService(GameGui);
            BindDalamudService(GameNetwork);
            BindDalamudService(JobGauges);
            BindDalamudService(KeyState);
            BindDalamudService(LibcFunction);
            BindDalamudService(ObjectTable);
            BindDalamudService(PartyFinderGui);
            BindDalamudService(PartyList);
            BindDalamudService(SigScanner);
            BindDalamudService(TargetManager);
            BindDalamudService(ToastGui);

            var config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Bind<Configuration>().ToConstant(config).InScope(_ => _scope);

            BindSingleton<TrackerWidget>();
            BindSingleton<ActionLibrary>();
            BindSingleton<IconManager>();
            BindSingleton<ConfigurationWindow>();
            BindSingleton<PartyListHUD>();

            BindSingleton<IActionTrackerSource, NetworkActionTrackerSource>();

            BindSingleton<IActionTracker, ActionTrackerImpl>(ActionTracker.regular);
            BindSingleton<IActionTracker, MockActionTrackerImpl>(ActionTracker.testingDisplay);
        }

        private void BindDalamudService<T>(T instance)
        {
            Bind<T>().ToConstant(instance).InTransientScope();
        }

        private void BindSingleton<T>()
        {
            Bind<T>().To<T>().InScope(_ => _scope);
        }

        private void BindSingleton<TInterface, TImpl>(string? name=null) where TImpl: TInterface
        {
            var binding = Bind<TInterface>().To<TImpl>().InScope(_ => _scope);
            if (name != null)
            {
                binding.Named(name);
            }
        }
    }
}