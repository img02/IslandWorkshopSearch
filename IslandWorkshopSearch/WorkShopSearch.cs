using Dalamud.Game.Addon.Lifecycle;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using IslandWorkshopSearch.Windows;
using IslandWorkshopSearch.Windows.ViewModels;

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
        [PluginService] public static IAddonEventManager AddonEventManager { get; private set; }
        [PluginService] private IAddonLifecycle AddonLifecycle { get; init; }

        public WorkShopSearch(
             IPluginLog logger,
             IAddonLifecycle lifecycle
            )
        {
            PluginLog.Logger = logger;
            AddonLifecycle!.RegisterListener(AddonEvent.PostSetup, Search.AddOnName, Search.PostAgendaWindowSetUp);
            MainWindow = new MainWindow();
            PluginInterface!.UiBuilder.Draw += DrawUI;
        }

        private void DrawUI() => this.MainWindow.Draw();

        public void Dispose()
        {
            AddonLifecycle!.UnregisterListener(AddonEvent.PostSetup, Search.AddOnName, Search.PostAgendaWindowSetUp);
        }
    }
}
