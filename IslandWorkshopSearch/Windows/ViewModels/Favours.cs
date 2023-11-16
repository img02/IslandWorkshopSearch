using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using IslandWorkshopSearch.Managers.WorkshopCrafts;

namespace IslandWorkshopSearch.Windows.ViewModels
{
    internal unsafe class Favours
    {
        public readonly string AddOnName = "MJINekomimiRequest";
        private readonly string[] current = new string[3];
        private readonly string[] next = new string[3];

        public float Scale => UpdateScale();

        public AtkUnitBase* GetUI() => (AtkUnitBase*)WorkShopSearch.GameGui.GetAddonByName(AddOnName);
        public bool UiExists() => GetUI() != null;


        public Vector2 GetWorkshopAgendaGuiPos()
        {
            var ui = GetUI();
            if (ui == null) return Vector2.Zero;
            return new Vector2(ui->GetX(), ui->GetY());
        }

        /// <summary>
        /// favour scale uses ui scale for button positioning inside the GUI
        /// </summary>
        /// <returns></returns>
        private float UpdateScale()
        {
            var ui = GetUI();
            return ui == null ? 1f : ui->Scale ;
        }

        public string GetCurrent()
        {
            return GetCommandString(current);
        }

        public string GetNext()
        {
            return GetCommandString(next);
        }

        private string GetCommandString(string[] favours)
        {
            WorkshopCrafts.ConvertNamesToEnglish(favours);

            for (var i = 0; i < favours.Length; i++)
            {
                if (favours[i].Contains("Isleworks "))
                {
                    favours[i] = favours[i][10..];
                }
                if (favours[i].Contains(OCName.MammetAward.Original))
                {
                    favours[i] = OCName.MammetAward.OCName;
                }
            }

            return $"/favors favor1:{favours[0]} favor2:{favours[1]} favor3:{favours[2]}";
        }

        public void PostRefresh(AddonEvent _, AddonArgs __) => UpdateFavours();

        private void UpdateFavours()
        {
            var ui = GetUI();
            if (ui == null) return;
            if (ui->AtkValuesCount == 0)
            {
                PluginLog.Error("AtkValuesCount is 0, idk");
                return;
            };

            var values = ui->AtkValues;
            var baseIndex = 6;

            try
            {
                current[0] = Marshal.PtrToStringUTF8((nint)(values[baseIndex].String))!;
                current[1] = Marshal.PtrToStringUTF8(((nint)values[baseIndex += 8].String))!;
                current[2] = Marshal.PtrToStringUTF8(((nint)values[baseIndex += 8].String))!;

                next[0] = Marshal.PtrToStringUTF8((nint)values[baseIndex += 8].String)!;
                next[1] = Marshal.PtrToStringUTF8((nint)values[baseIndex += 8].String)!;
                next[2] = Marshal.PtrToStringUTF8((nint)values[baseIndex += 8].String)!;
            }
            catch (Exception e)
            {
                PluginLog.Error(e.ToString());
            }

#if DEBUG
            for (var i = 0; i < 3; i++)
            {
                PluginLog.Warning($"{current[i]} : {next[i]}");
            }
#endif
        }
    }
}
