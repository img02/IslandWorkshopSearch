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
        [PluginService] private IDalamudPluginInterface PluginInterface { get; init; }

        private MainWindow MainWindow { get; init; }

        [PluginService] public static IGameGui? GameGui { get; private set; }
        [PluginService] public static IDataManager? DataManager { get; private set; }
        [PluginService] public static IClientState? ClientState { get; private set; }
        [PluginService] public static IAddonEventManager? AddonEventManager { get; private set; }
        [PluginService] private IAddonLifecycle AddonLifecycle { get; init; }

        private Favours favours { get; set; }
        private Search search { get; set; }

        public WorkShopSearch(
             IPluginLog logger,
             IAddonLifecycle lifecycle
            )
        {
            PluginLog.Logger = logger;

            search = new Search();
            favours = new Favours();

            AddonLifecycle!.RegisterListener(AddonEvent.PostSetup, search.AddOnName, search.PostAgendaWindowSetUp);
            AddonLifecycle!.RegisterListener(AddonEvent.PostRefresh, favours.AddOnName, favours.PostRefresh);
            MainWindow = new MainWindow(search, favours);
            PluginInterface!.UiBuilder.Draw += DrawUI;
        }

        private void DrawUI() => this.MainWindow.Draw();

        public void Dispose()
        {
            AddonLifecycle!.UnregisterListener(AddonEvent.PostSetup, search.AddOnName, search.PostAgendaWindowSetUp);
            AddonLifecycle!.UnregisterListener(AddonEvent.PostRefresh, favours.AddOnName, favours.PostRefresh);
        }
    }
}
