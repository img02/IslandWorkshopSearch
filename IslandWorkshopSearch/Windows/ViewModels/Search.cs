using Dalamud.Game.Addon.Events;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using IslandWorkshopSearch.Managers.WorkshopCrafts;
using IslandWorkshopSearch.Utility;
using IslandWorkshopSearch.Windows.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IslandWorkshopSearch.Windows.ViewModels
{
    internal unsafe class Search
    {
        public float Scale => UpdateScale();

        public readonly string AddOnName = "MJICraftScheduleSetting";
        public const uint TreeListNodeId = 7;
        private const uint ScheduleButtonNodeId = 55;

        public string SearchInput = string.Empty;
        private string[] incrementalSearchTerms = Array.Empty<string>();

        private AtkComponentTreeList* TreeList;
        private bool AutoSelectEnabled = true;

        public void UpdateSearch(string searchInput)
        {
            SearchInput = searchInput;
            if (AutoSelectEnabled)
            {
                AutoSelectItem();
            }
        }

        private void AutoSelectItem()
        {
            if (string.IsNullOrWhiteSpace(SearchInput)) return;

            var ui = GetUI();
            if (ui == null) return;

            if (!ui->IsVisible || ui->UldManager.LoadedState != AtkLoadState.Loaded) return;

            TreeList = GetTreeList(ui);
            if (TreeList == null) return;

            if (TreeList->Items.LongCount == 0) return;

            var entries = new List<(int Index, string ItemName)>();
            var searchTerms = GetSearchTerms();
            if (searchTerms.Length == 0) return;

            var currentTerm = searchTerms[0].Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(currentTerm)) return;

            for (var i = 0; i < TreeList->Items.LongCount; i++)
            {
                var item = TreeList->Items[i].Value;
                if (item != null && item->UIntValues.LongCount >= 3 && item->UIntValues[0] != (uint)AtkComponentTreeListItemType.CollapsibleGroupHeader)
                {
                    var rowId = item->UIntValues[2];
                    var craftItem = WorkshopCrafts.Crafts.FirstOrDefault(c => c.ID == rowId);
                    if (craftItem != null)
                    {
                        entries.Add((i, craftItem.Name.ToLowerInvariant()));
                    }
                }
            }

            var exactMatch = entries.FirstOrDefault(e => e.ItemName == currentTerm);
            if (exactMatch != default)
            {
                SelectAndExpandItem(exactMatch.Index);
                return;
            }

            var partialMatch = entries.FirstOrDefault(e => e.ItemName.Contains(currentTerm));
            if (partialMatch != default)
            {
                SelectAndExpandItem(partialMatch.Index);
            }
        }

        private void SelectAndExpandItem(int index)
        {
            var item = TreeList->GetItem(index);
            if (item == null) return;

            for (var i = index; i >= 0; i--)
            {
                var headerItem = TreeList->Items[i].Value;
                if (headerItem != null && headerItem->UIntValues.LongCount >= 1 && headerItem->UIntValues[0] == (uint)AtkComponentTreeListItemType.CollapsibleGroupHeader)
                {
                    TreeList->ExpandGroupExclusively(headerItem, false);
                    TreeList->LayoutRefreshPending = true;
                    break;
                }
            }

            TreeList->SelectItem(index, true);
        }

        private float CalculateFuzzyScore(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0;

            str1 = str1.ToLower();
            str2 = str2.ToLower();

            if (str1.Contains(str2))
                return 1.0f;

            var distance = LevenshteinDistance(str1, str2);
            var maxLength = Math.Max(str1.Length, str2.Length);
            return 1.0f - ((float)distance / maxLength);
        }

        private int LevenshteinDistance(string str1, string str2)
        {
            var n = str1.Length;
            var m = str2.Length;
            var d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (var i = 0; i <= n; i++)
                d[i, 0] = i;
            for (var j = 0; j <= m; j++)
                d[0, j] = j;

            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j <= m; j++)
                {
                    var cost = (str2[j - 1] == str1[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        #region atk stuff

        public AtkUnitBase* GetUI() => Common.GetUI(AddOnName);
        public bool UiExists() => GetUI() != null;
        private AtkComponentTreeList* GetTreeList(AtkUnitBase* ui) => (AtkComponentTreeList*)Common.GetWorkshopAgendaTreeList(ui);

        public bool IsIncrementalSearch => incrementalSearchTerms.Length > 1;

        #endregion

        /// <summary>
        /// search scale uses globalscale for positioning and size
        /// </summary>
        /// <returns></returns>
        private float UpdateScale() => ImGuiHelpers.GlobalScale;

        public Vector2 GetWorkshopAgendaGuiPos()
        {
            var ui = GetUI();
            if (ui == null) return Vector2.Zero;
            return new Vector2(ui->GetX(), ui->GetY());
        }

        public void SearchWorkshop()
        {
            ResetTabColours();
            if (SearchInput.Length == 0)
            {
                incrementalSearchTerms = Array.Empty<string>();
                TextNodeColour.ResetWorkshopTextColours(GetUI());

                return;
            }
            var four = false;
            var six = false;
            var eight = false;

            TextNodeColour.ChangeAllWorkshopTextColourGrey(GetUI());
            var foundNames = MatchItemsAndFilterHours(GetSearchTerms(), ref four, ref six, ref eight);
            FilterWorkshopItemList(foundNames);
            HighlightTabs(four, six, eight);
        }

        private void HighlightTabs(bool four, bool six, bool eight)
        {
            var ui = GetUI();
            if (ui == null) return;
            var treeList = GetTreeList(ui);
            if (treeList == null) return;

            if (four)
            {
                var fourHourList = ((AtkComponentBase*)treeList)->UldManager.NodeList[28];
                var fourText = ((AtkComponentNode*)fourHourList)->Component->UldManager.NodeList[2]->GetAsAtkTextNode();
                TextNodeColour.ChangeTextColourGold(fourText);
            }
            if (six)
            {
                var sixHourList = ((AtkComponentBase*)treeList)->UldManager.NodeList[29];
                var sixText = ((AtkComponentNode*)sixHourList)->Component->UldManager.NodeList[2]->GetAsAtkTextNode();
                TextNodeColour.ChangeTextColourGold(sixText);
            }
            if (eight)
            {
                var eightHourList = ((AtkComponentBase*)treeList)->UldManager.NodeList[30];
                var eightText = ((AtkComponentNode*)eightHourList)->Component->UldManager.NodeList[2]->GetAsAtkTextNode();
                TextNodeColour.ChangeTextColourGold(eightText);
            }
        }

        private void ResetTabColours()
        {
            var ui = GetUI();
            if (ui == null) return;
            var treeList = GetTreeList(ui);
            if (treeList == null) return;

            for (var i = 28; i <= 30; i++)
            {
                var list = ((AtkComponentBase*)treeList)->UldManager.NodeList[i];
                var text = ((AtkComponentNode*)list)->Component->UldManager.NodeList[2]->GetAsAtkTextNode();
                TextNodeColour.ChangeTextColourWhite(text);
            }
        }

        #region search  string filtering
        private string[] GetSearchTerms()
        {
            // if it's got a pipe - grouped search, empty out incremental search terms
            if (SearchInput.Contains('|') || SearchInput.Contains('｜'))
            {
#if DEBUG
                PluginLog.Debug("grouped search : " + SearchInput);
#endif
                incrementalSearchTerms = Array.Empty<string>();
                return GroupSearch.GetTerms(SearchInput); ;
            }
            if (SearchInput.Contains('\n')) FilterOverseasCasualsSearch(); //format copy n paste into csv then incr.
#if DEBUG
            PluginLog.Debug("step-by-step search");
#endif
            return IncrementalSearch.GetTerms(SearchInput, ref incrementalSearchTerms);
        }

        private void FilterOverseasCasualsSearch()
        {
            //matches |:OC_IconName: [real name] (4h)| format of Overseas Casuals bot
            var regx = new Regex(":.*?: (.*?) \\(\\dh\\)");
            var regxFiltered = new List<string>();

            foreach (var c in regx.Matches(SearchInput).Cast<Match>())
            {
                // PluginLog.Debug(c.Groups[1].ToString()+"|");
                try
                {
                    regxFiltered.Add(c.Groups[1].ToString());
                }
                catch (Exception e)
                {
                    PluginLog.Error(e.StackTrace!);
                }
            }
            SearchInput = string.Join(',', WorkshopCrafts.LocaliseNames(regxFiltered).ToArray());
        }


        #endregion

        private string[] MatchItemsAndFilterHours(string[] toMatch, ref bool four, ref bool six, ref bool eight)
        {
            var searchedFor = new List<string>();
            //PluginLog.Debug("----------------------");            

            foreach (var name in toMatch)
            {
                if (name.Length == 0) continue;
                foreach (var c in WorkshopCrafts.Crafts.Where(c => c.Name.ToLowerInvariant().Contains(name.Trim().ToLowerInvariant())))
                {
                    searchedFor.Add(c.Name);
                    switch (c.Hours)
                    {
                        case 4: four = true; break;
                        case 6: six = true; break;
                        case 8: eight = true; break;
                    }
                    //PluginLog.Debug($"Matched: {c.Name}|{name}");
                }
            }
            //PluginLog.Debug($"{searchedFor.Count}");

            return searchedFor.ToArray();
        }

        public void FilterWorkshopItemList(string[] toSearch)
        {
            var ui = GetUI();
            if (ui == null) return;
            var treeList = GetTreeList(ui);
            if (treeList == null) return;

            foreach (var name in toSearch)
            {
                for (var i = 1; i <= 27; i++)
                {
                    var listItemNode = ((AtkComponentBase*)treeList)->UldManager.NodeList[i];
                    var textNode = ((AtkComponentNode*)listItemNode)->Component->UldManager.NodeList[3]->GetAsAtkTextNode();
                    if (textNode == null) continue;
                    if (textNode->NodeText.ToString() != name) continue;
                    else TextNodeColour.ChangeTextColourGold(textNode);
                }
            }
        }

        public void PostAgendaWindowSetUp(AddonEvent _, AddonArgs __)
        {
            PluginLog.Debug("yes it is setup");
            var ui = GetUI();
            var btn = ui->GetNodeById(ScheduleButtonNodeId);
            //automatically removed
            WorkShopSearch.AddonEventManager!.AddEvent((nint)ui, (nint)btn, AddonEventType.ButtonClick, ScheduleButtonClicked);

            if (!string.IsNullOrEmpty(SearchInput))
            {
                SearchWorkshop();
                
                Task.Delay(100).ContinueWith(_ =>
                {
                    AutoSelectItem();
                });
            }
        }

        private void ScheduleButtonClicked(AddonEventType _, IntPtr __, IntPtr ___)
        {
            PluginLog.Debug("yes the button was clicked yo");
            if (incrementalSearchTerms.Length == 0) return;
            SearchInput = string.Join(",", incrementalSearchTerms.Skip(1));
        }

        public void HandleEnterKey()
        {
            var ui = GetUI();
            if (ui == null) return;

            var evt = new AtkEvent();
            ui->ReceiveEvent(AtkEventType.ButtonClick, 6, &evt);

            if (incrementalSearchTerms.Length > 0)
            {
                SearchInput = string.Join(",", incrementalSearchTerms.Skip(1));
                incrementalSearchTerms = GetSearchTerms();
            }
        }
    }
}
