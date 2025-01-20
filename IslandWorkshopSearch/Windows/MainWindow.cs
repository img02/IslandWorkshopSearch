using Dalamud.Interface;
using ImGuiNET;
using IslandWorkshopSearch.Utility;
using IslandWorkshopSearch.Windows.ViewModels;
using System;
using System.Linq;
using System.Numerics;

namespace IslandWorkshopSearch.Windows;

public unsafe class MainWindow
{
    private readonly Search search;
    private readonly Favours favours;
    private bool shouldFocus = true;

    internal MainWindow(Search search, Favours favours)
    {
        this.search = search;
        this.favours = favours;
    }

    public void Draw()
    {
        DrawSearchBar();
        DrawFavourButtons();
    }

    #region workshop search bar

    private void DrawSearchBar()
    {
        if (!search.UiExists()) return;

        var searchBarsize = new Vector2(260f * search.Scale, 24f * search.Scale);

        var winPos = new Vector2(search.GetWorkshopAgendaGuiPos().X + 26,
                                 search.GetWorkshopAgendaGuiPos().Y - (search.Scale * 20));

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

            ImGui.PushFont(UiBuilder.MonoFont);
            
            if (ImGui.IsWindowAppearing())
            {
                shouldFocus = true;
            }

            if (shouldFocus)
            {
                ImGui.SetKeyboardFocusHere();
                shouldFocus = false;
            }
            
            var currentInput = search.SearchInput;
            var searchChanged = ImGui.InputTextMultiline("dsadsadsadasdsa##cosmetic tag info", 
                ref search.SearchInput, 
                2000, 
                searchBarsize, 
                ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll);

            if (currentInput != search.SearchInput || 
                ImGui.IsKeyPressed(ImGuiKey.Comma) || 
                (ImGui.GetIO().KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.V)) ||
                ImGui.IsWindowFocused())
            {
                search.UpdateSearch(search.SearchInput);
                shouldFocus = true;
            }
            
            if (searchChanged && 
                (ImGui.GetIO().KeyCtrl || ImGui.GetIO().KeysDown[(int)ImGuiKey.LeftCtrl] || ImGui.GetIO().KeysDown[(int)ImGuiKey.RightCtrl]))
            {
                search.HandleEnterKey();
                shouldFocus = true;
            }
            if (search.IsIncrementalSearch) DrawTextWrappedInputDisplay();
            ImGui.PopFont();

            search.SearchWorkshop();

            ImGui.End();
            ImGui.PopStyleVar(4);
        }
    }

    private void DrawTextWrappedInputDisplay()
    {
        var size = new Vector2(200f * search.Scale, 500f * search.Scale);
        var winPos = new Vector2(search.GetWorkshopAgendaGuiPos().X - (size.X - 3), search.GetWorkshopAgendaGuiPos().Y + 10);

        ImGui.SetNextWindowSize(size);
        ImGui.SetNextWindowPos(winPos);

        if (ImGui.Begin("display", ImGuiWindowFlags.NoMove |
             ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize))
        {
            var (first, rest) = GetFormattedDisplayText();
            ImGui.TextColored(new(1f, 215 / 255f, 0f, 1), first);
            ImGui.TextWrapped(rest);
            ImGui.End();
        }
    }

    private (string first, string rest) GetFormattedDisplayText()
    {
        var split = search.SearchInput.Split(',');
        return (split[0], string.Join('\n', split.Skip(1)));

    }

    #endregion

    #region favours 

    private void DrawFavourButtons()
    {
        if (!favours.UiExists()) return;
        var scale = favours.Scale;
        var winPos = new Vector2(favours.GetWorkshopAgendaGuiPos().X + (scale * 100),
                                favours.GetWorkshopAgendaGuiPos().Y + (scale * 180));

        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);

        ImGui.SetNextWindowSize(new Vector2(400 * scale, 250 * scale));
        ImGui.SetNextWindowPos(winPos);

        if (ImGui.Begin("noonewillseethis", ImGuiWindowFlags.NoMove |
             ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize))
        {
            ImGui.SetCursorPosY(0);
            ImGui.SetCursorPosX(0);

            ImGui.PushFont(UiBuilder.MonoFont);

            FavorButtons(scale);

            ImGui.PopFont();

            ImGui.End();
            ImGui.PopStyleVar(4);
        }
    }

    // i put in too much effort making these ugly buttons

    private void FavorButtons(float scale)
    {
        var size = new Vector2(70 * scale, 22 * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 30);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2);

        ImGui.PushStyleColor(ImGuiCol.Button, ImGuiUtil.AltColour);
        ImGui.PushStyleColor(ImGuiCol.Border, ImGuiUtil.MainColour);
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiUtil.MainColour);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGuiUtil.AltColour);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGuiUtil.AltColour);

        ImGui.SetCursorPosX(200 * scale);
        ImGui.SetCursorPosY(19 * scale);

        CustomButton(size, 0, "current", favours.GetCurrent);
        ImGuiUtil.ImGui_HoveredToolTip("get bot command for the Overseas Casuals Discord");

        ImGui.SetCursorPosX(200 * scale);
        ImGui.SetCursorPosY(209 * scale);
        CustomButton(size, 1, "next", favours.GetNext);
        ImGuiUtil.ImGui_HoveredToolTip("get bot command for the Overseas Casuals Discord");

        ImGui.PopStyleColor(5);
        ImGui.PopStyleVar(2);
    }

    private void CustomButton(Vector2 size, int id, string label, Func<string> fn)
    {
        var colToPop = 1;

        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 999);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGuiUtil.MainColour);

        if (ImGui.BeginChild($"favourbtn##{id}", size))
        {
            if (ImGui.IsWindowFocused())
            {
                if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, ImGuiUtil.MainColour);
                    ImGui.PushStyleColor(ImGuiCol.Border, ImGuiUtil.AltColour);
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiUtil.AltColour);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGuiUtil.MainColour);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGuiUtil.MainColour);
                    colToPop += 5;
                }
            }

            if (ImGui.Button($"{label}##{id}", size))
            {
                ImGui.SetClipboardText(fn());
            }

            ImGui.PopStyleColor(colToPop);
            ImGui.PopStyleVar(1);
            ImGui.EndChild();
        }
    }
    #endregion
}

