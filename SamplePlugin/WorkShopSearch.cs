using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using IslandWorkshopSearch.Windows;

namespace IslandWorkshopSearch
{
    public sealed class WorkShopSearch : IDalamudPlugin
    {
        public string Name => "Island Workshop Search";
        [PluginService] private DalamudPluginInterface PluginInterface { get; init; }

        private MainWindow MainWindow { get; init; }

        [PluginService] public static IGameGui GameGui { get; private set; }
        [PluginService] public static IDataManager DataManager { get; private set; }
        [PluginService] public static IClientState ClientState { get; private set; }

        public WorkShopSearch(
            DalamudPluginInterface pluginInterface,
             IClientState clientState,
             IPluginLog logger
            )
        {
            PluginLog.Logger = logger;
            MainWindow = new MainWindow();
            PluginInterface!.UiBuilder.Draw += DrawUI;
        }

        private void DrawUI() => this.MainWindow.Draw();

        public void Dispose() { }
    }
}
