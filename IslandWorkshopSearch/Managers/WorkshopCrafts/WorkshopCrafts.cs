using IslandWorkshopSearch.Managers.WorkshopCrafts.Models;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;
using System.Linq;

namespace IslandWorkshopSearch.Managers.WorkshopCrafts
{
    internal class WorkshopCrafts
    {
        public static readonly IList<WorkshopCraftsItem> Crafts = GetWorkshopItemsList();

        private static IList<WorkshopCraftsItem> GetWorkshopItemsList()
        {
            WorkShopSearch.DataManager.Excel.RemoveSheetFromCache<MJICraftworksObject>();
            var craftworkItems = WorkShopSearch.DataManager.GetExcelSheet<MJICraftworksObject>(WorkShopSearch.ClientState.ClientLanguage);
            var crafts = new List<WorkshopCraftsItem>();

            foreach (var cwc in craftworkItems!)
            {
                if (cwc!.Item!.Value!.RowId == 0) continue;               
                crafts.Add(new WorkshopCraftsItem(cwc!.Item!.Value!.Name, cwc!.CraftingTime, cwc!.RowId)); 
            }
            return crafts;
        }

        public static IList<string> LocaliseNames(IList<string> regxFilteredInput)
        {
            var items = new List<string>();
            WorkShopSearch.DataManager.Excel.RemoveSheetFromCache<MJICraftworksObject>();            
            var craftworkItems = WorkShopSearch.DataManager.GetExcelSheet<MJICraftworksObject>(Dalamud.ClientLanguage.English);

            foreach (var search in regxFilteredInput)
            {
                var lowercaseSearch = search.ToLowerInvariant();
                foreach (var cwc in craftworkItems!)
                {
                    if (cwc!.Item!.Value!.RowId == 0) continue;
                    //PluginLog.Warning(search);
                    // are there any conflicting item names? don't think so but idk                    
                    var itemName = cwc.Item!.Value!.Name.ToString().ToLowerInvariant();
                    if (itemName == OCName.MammetAward.Original) itemName = OCName.MammetAward.OCName;
                    if (!itemName.Contains(lowercaseSearch)) continue;

                    //PluginLog.Error(cwc.Item.Value.Name.ToString());

                    var item = Crafts.First(i => i.ID == cwc.RowId);
                    //PluginLog.Warning(item.Name);
                    items.Add(item.Name);
                }
            }
            return items;
        }
    }
}
