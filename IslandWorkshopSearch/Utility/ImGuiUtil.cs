using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Numerics;

namespace IslandWorkshopSearch.Utility
{
    internal class ImGuiUtil
    {
        public static readonly uint AltColour = ImGui.ColorConvertFloat4ToU32(new Vector4(172f / 255, 155f / 255, 132f / 255, 1));
        public static readonly uint MainColour = ImGui.ColorConvertFloat4ToU32(new Vector4(46 / 255f, 34 / 255f, 28 / 255f, 1));

        public static void DoStuffWithMonoFont(Action function)
        {
            ImGui.PushFont(UiBuilder.MonoFont);
            function();
            ImGui.PopFont();
        }

        public static void ImGui_HoveredToolTip(string msg, Vector4 bg)
        {
            ImGui.PushStyleColor(ImGuiCol.PopupBg, bg);
            ImGui_HoveredToolTip(msg);
            ImGui.PopStyleColor();
        }

        public static void ImGui_HoveredToolTip(string msg)
        {
            if (ImGui.IsItemHovered())
            {

                ImGui.PushStyleColor(ImGuiCol.Text, AltColour);
                ImGui.PushStyleColor(ImGuiCol.Border, AltColour);
                ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 1f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(6f, 6f));

                ImGui.BeginTooltip();
                ImGui.Text(msg);
                ImGui.EndTooltip();

                ImGui.PopStyleColor(2);
                ImGui.PopStyleVar(2);
            }
        }
    }
}
