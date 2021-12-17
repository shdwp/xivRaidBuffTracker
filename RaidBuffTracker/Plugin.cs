using Dalamud.Game;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using Ninject;

namespace RaidBuffTracker
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Sample Plugin";

        private readonly App _app;

        public Plugin(DalamudPluginInterface pluginInterface, GameGui gameGui, SigScanner sigScanner)
        {
            var settings = new NinjectSettings
            {
                LoadExtensions = false,
            };

            var module = new Module(pluginInterface);
            Module.shared = new StandardKernel(settings, module);

            _app = Module.shared.Get<App>();
        }

        public void Dispose()
        {
            _app.Dispose();
            Module.shared.Dispose();
        }
    }
}