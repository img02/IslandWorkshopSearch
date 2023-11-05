using Dalamud.Interface;
using ImGuiNET;
using IslandWorkshopSearch.Windows.ViewModels;
using System.Numerics;

namespace IslandWorkshopSearch.Windows;

public unsafe class MainWindow
{
    public MainWindow()
    {
    }

    private string searchInput = string.Empty;
    private float searchBarWidth = 260;
    public void Draw()
    {
        if (!Search.UiExists()) return;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);

        ImGui.SetNextWindowSize(new Vector2(searchBarWidth * Search.Scale, 22 * Search.Scale));
        ImGui.SetNextWindowPos(Search.GetWorkshopAgendaGuiWindowPosWowThisIsAReallyLongName());

        if (ImGui.Begin("noonewillseethis", ImGuiWindowFlags.NoMove |
             ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize))
        {
            ImGui.SetCursorPosY(0);
            ImGui.SetCursorPosX(0);
            ImGui.SetNextItemWidth(searchBarWidth * Search.Scale);
            ImGui.SetWindowFontScale(Search.Scale); // this is blurry lol        
            ImGui.PushFont(UiBuilder.MonoFont);
            ImGui.InputTextWithHint("##idkidkidk", "Search", ref Search.SearchInput, 500);
            ImGui.PopFont();

            Search.SearchWorkshop();

            ImGui.End();
            ImGui.PopStyleVar(4);
        }
    }
}
