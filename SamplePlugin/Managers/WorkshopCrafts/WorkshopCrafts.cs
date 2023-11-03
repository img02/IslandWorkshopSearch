using IslandWorkshopSearch.Managers.WorkshopCrafts.Models;
using Lumina.Excel.GeneratedSheets2;
using System.Collections.Generic;

namespace IslandWorkshopSearch.Managers.WorkshopCrafts
{
    internal class WorkshopCrafts
    {
        public static List<WorkshopCraftsItem> GetWorkshopItemsList()
        {
            var craftworkItems = WorkShopSearch.DataManager.GetExcelSheet<MJICraftworksObject>(WorkShopSearch.ClientState.ClientLanguage);
            var crafts = new List<WorkshopCraftsItem>();

            foreach (var cwc in craftworkItems)
            {
                if (cwc!.Item!.Value!.RowId == 0) continue;
                crafts.Add(new WorkshopCraftsItem(cwc!.Item!.Value!.Name, cwc!.CraftingTime));
            }
            return crafts;
        }
    }
}
