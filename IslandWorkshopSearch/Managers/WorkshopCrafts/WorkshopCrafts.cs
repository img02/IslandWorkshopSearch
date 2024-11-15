using IslandWorkshopSearch.Managers.WorkshopCrafts.Models;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Linq;

namespace IslandWorkshopSearch.Managers.WorkshopCrafts
{
    internal class WorkshopCrafts
    {
        public static readonly IList<WorkshopCraftsItem> Crafts = GetWorkshopItemsList();

        private static IList<WorkshopCraftsItem> GetWorkshopItemsList()
        {
            //WorkShopSearch.DataManager!.Excel.RemoveSheetFromCache<MJICraftworksObject>();
            var craftworkItems = WorkShopSearch.DataManager!.GetExcelSheet<MJICraftworksObject>(WorkShopSearch.ClientState!.ClientLanguage);
            var crafts = new List<WorkshopCraftsItem>();

            foreach (var cwc in craftworkItems!)
            {
                if (cwc!.Item!.Value!.RowId == 0) continue;
                crafts.Add(new WorkshopCraftsItem(cwc!.Item!.Value!.Name.ToString(), cwc!.CraftingTime, cwc!.RowId));
            }
            return crafts;
        }

        /// <summary>
        /// gets names based on client language from English input (OC discord bot recomendations)
        /// </summary>
        /// <param name="regxFilteredInput"></param>
        /// <returns></returns>
        public static IList<string> LocaliseNames(IList<string> regxFilteredInput)
        {
            var items = new List<string>();
            //WorkShopSearch.DataManager.Excel.RemoveSheetFromCache<MJICraftworksObject>();
            var craftworkItems = WorkShopSearch.DataManager!.GetExcelSheet<MJICraftworksObject>(Dalamud.Game.ClientLanguage.English);

            foreach (var search in regxFilteredInput)
            {
                var lowercaseSearch = search.ToLowerInvariant();

                foreach (var cwc in craftworkItems!)
                {
                    if (cwc!.Item!.Value!.RowId == 0) continue;
                    
                    var itemName = cwc.Item!.Value!.Name.ToString();

                    // OC bot output name diff
                    if (itemName == OCName.MammetAward.Original) itemName = OCName.MammetAward.OCName;

                    //
                    if (!itemName.ToLowerInvariant().EndsWith(lowercaseSearch)) continue;

                    var item = Crafts.First(i => i.ID == cwc.RowId);
                    items.Add(item.Name);
                }
            }
            return items;
        }

        public static void ConvertNamesToEnglish(string[] favours)
        {
            //WorkShopSearch.DataManager.Excel.RemoveSheetFromCache<MJICraftworksObject>();
            var craftworkItems = WorkShopSearch.DataManager!.GetExcelSheet<MJICraftworksObject>(Dalamud.Game.ClientLanguage.English);

            foreach (var craft in Crafts)
            {
                for (var i = 0; i < favours.Length; i++)
                {
                    if (!craft.Name.Equals(favours[i])) continue;
                    favours[i] = craftworkItems!.First(c => c.RowId == craft.ID).Item.Value!.Name.ToString();
                }
            }
        }
    }
}
