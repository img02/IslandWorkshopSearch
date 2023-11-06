using Dalamud.Interface;
using ImGuiNET;
using IslandWorkshopSearch.Windows.ViewModels;
using System.Linq;
using System.Numerics;

namespace IslandWorkshopSearch.Windows;

public unsafe class MainWindow
{
    public MainWindow()
    {
    }

    private string searchInput = string.Empty;
    public void Draw()
    {
        //Test();
        if (!Search.UiExists()) return;

        var searchBarsize = new Vector2(260f * Search.Scale, 22f * Search.Scale);
        //if (Search.SearchInput.Length > 0) size = new(Search.SearchInput.Length * 8 * Search.Scale , 26 * Search.Scale );

        var winPos = new Vector2(Search.GetWorkshopAgendaGuiPos().X + (Search.Scale * 26),
                                 Search.GetWorkshopAgendaGuiPos().Y - (Search.Scale * 20));

        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);

        ImGui.SetNextWindowSize(searchBarsize);
        ImGui.SetNextWindowPos(winPos);

        if (ImGui.Begin("noonewillseethis", ImGuiWindowFlags.NoMove |
             ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize))
        {
            ImGui.SetCursorPosY(0);
            ImGui.SetCursorPosX(0);
            ImGui.SetWindowFontScale(Search.Scale); // this is blurry lol        
            ImGui.PushFont(UiBuilder.MonoFont);
            ImGui.InputTextMultiline("dsadsadsadasdsa##cosmetic tag info", ref Search.SearchInput, 2000, searchBarsize);
            if (Search.IsIncrementalSearch) DrawTextWrappedInputDisplay();
            ImGui.PopFont();

            Search.SearchWorkshop();

            ImGui.End();
            ImGui.PopStyleVar(4);
        }
    }

    private void DrawTextWrappedInputDisplay()
    {
        var size = new Vector2(200f * Search.Scale, 500f * Search.Scale);
        var winPos = new Vector2(Search.GetWorkshopAgendaGuiPos().X - (size.X - 3), Search.GetWorkshopAgendaGuiPos().Y + 10);

        ImGui.SetNextWindowSize(size);
        ImGui.SetNextWindowPos(winPos);

        if (ImGui.Begin("display", ImGuiWindowFlags.NoMove |
             ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize))
        {
            ImGui.SetWindowFontScale(Search.Scale); // this is blurry lol
            var (first, rest) = GetFormattedDisplayText();
            ImGui.TextColored(new(1f, 215 / 255f, 0f, 1), first);
            ImGui.TextWrapped(rest);
            ImGui.End();
        }
    }

    private (string first, string rest) GetFormattedDisplayText()
    {
        var split = Search.SearchInput.Split(',');
        return (split[0], string.Join('\n', split.Skip(1)));

    }
}

