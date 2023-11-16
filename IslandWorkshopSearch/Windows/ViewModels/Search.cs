using Dalamud.Game.Addon.Events;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using IslandWorkshopSearch.Managers.WorkshopCrafts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace IslandWorkshopSearch.Windows.ViewModels
{
    internal unsafe class Search
    {
        public  float Scale => UpdateScale();

        public readonly string AddOnName = "MJICraftScheduleSetting";
        private const uint TreeListNodeId = 7;
        private const uint ScheduleButtonNodeId = 55;

        public string SearchInput = string.Empty;
        private string[] incrementalSearchTerms = Array.Empty<string>();

        private readonly Vector4 gold = new(255, 215, 0, 255);
        private readonly Vector4 grey = new(200, 200, 200, 255);
        private readonly Vector4 white = new(255, 255, 255, 255);
        private readonly Vector4 defaultColour = new(235, 225, 207, 255);

        public  void UpdateSearch(string searchInput) => SearchInput = searchInput;

        #region atk stuff

        public  AtkUnitBase* GetUI() => (AtkUnitBase*)WorkShopSearch.GameGui.GetAddonByName(AddOnName);
        public  bool UiExists() => GetUI() != null;
        private  AtkComponentList* GetTreeList(AtkUnitBase* ui)
        {
            var treeList = ui->GetComponentListById(TreeListNodeId);
            if (treeList == null) return null;
            if (treeList->AtkComponentBase.UldManager.NodeListCount < 27) return null;
            return treeList;
        }
        public  bool IsIncrementalSearch => incrementalSearchTerms.Length > 1;

        #endregion

        /// <summary>
        /// search scale uses globalscale for positioning and size
        /// </summary>
        /// <returns></returns>
        private  float UpdateScale() => ImGuiHelpers.GlobalScale;        

        public  Vector2 GetWorkshopAgendaGuiPos()
        {
            var ui = GetUI();
            if (ui == null) return Vector2.Zero;
            return new Vector2(ui->GetX(), ui->GetY());
        }

        public  void SearchWorkshop()
        {
            if (SearchInput.Length == 0)
            {
                incrementalSearchTerms = Array.Empty<string>();
                ResetTextColours();
                ResetTabColours();
                return;
            }
            var four = false;
            var six = false;
            var eight = false;
            ResetTabColours();
            ChangeAllTextColourGrey();
            var foundNames = MatchItemsAndFilterHours(GetSearchTerms(), ref four, ref six, ref eight);
            FilterWorkshopItemList(foundNames);
            HighlightTabs(four, six, eight);
        }

        private  void HighlightTabs(bool four, bool six, bool eight)
        {
            var ui = GetUI();
            if (ui == null) return;
            var treeList = GetTreeList(ui);
            if (treeList == null) return;

            if (four)
            {
                var fourHourList = treeList->AtkComponentBase.UldManager.NodeList[28];
                var fourText = ((AtkComponentNode*)fourHourList)->Component->UldManager.NodeList[2]->GetAsAtkTextNode();
                ChangeTextColour(fourText, gold);
            }
            if (six)
            {
                var sixHourList = treeList->AtkComponentBase.UldManager.NodeList[29];
                var sixText = ((AtkComponentNode*)sixHourList)->Component->UldManager.NodeList[2]->GetAsAtkTextNode();
                ChangeTextColour(sixText, gold);
            }
            if (eight)
            {
                var eightHourList = treeList->AtkComponentBase.UldManager.NodeList[30];
                var eightText = ((AtkComponentNode*)eightHourList)->Component->UldManager.NodeList[2]->GetAsAtkTextNode();
                ChangeTextColour(eightText, gold);
            }
        }

        private  void ResetTabColours()
        {
            var ui = GetUI();
            if (ui == null) return;
            var treeList = GetTreeList(ui);
            if (treeList == null) return;

            for (var i = 28; i <= 30; i++)
            {
                var list = treeList->AtkComponentBase.UldManager.NodeList[i];
                var text = ((AtkComponentNode*)list)->Component->UldManager.NodeList[2]->GetAsAtkTextNode();
                ChangeTextColour(text, white);
            }
        }

        #region search  string filtering
        private  string[] GetSearchTerms()
        {
            // if it's got a pipe - grouped search, empty out incremental search terms
            if (SearchInput.Contains('|') || SearchInput.Contains('｜'))
            {
#if DEBUG
                PluginLog.Debug("grouped search : " + SearchInput);
#endif
                incrementalSearchTerms = Array.Empty<string>();
                return GroupSearch();
            }
            if (SearchInput.Contains('\n')) OverseasCasualsSearch(); //format copy n paste into csv then incr.
#if DEBUG
            PluginLog.Debug("step-by-step search");
#endif
            return IncrementalSearch();
        }

        private  void OverseasCasualsSearch()
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

        private  string[] IncrementalSearch()
        {
            incrementalSearchTerms = SplitStringIncremental();
            return new string[] { incrementalSearchTerms[0] };
        }

        private  string[] SplitStringIncremental()
        {
            var splitSearch = SearchInput.Split(new Char[] { '|', '｜' });
            var incrementalSearch = new List<string>();

            foreach (var s in splitSearch)
            {
                var split = s.Split(new Char[] { ',', '、' });

                foreach (var item in split)
                {
                    incrementalSearch.Add(item);
                }
            }
            return incrementalSearch.ToArray();
        }

        private  string[] GroupSearch() => SearchInput.Split(new Char[] { '|', ',', '｜', '、' });

        #endregion

        private  string[] MatchItemsAndFilterHours(string[] toMatch, ref bool four, ref bool six, ref bool eight)
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

        public  void FilterWorkshopItemList(string[] toSearch)
        {
            var ui = GetUI();
            if (ui == null) return;
            var treeList = GetTreeList(ui);
            if (treeList == null) return;

            foreach (var name in toSearch)
            {
                for (var i = 1; i <= 27; i++)
                {
                    var listItemNode = treeList->AtkComponentBase.UldManager.NodeList[i];
                    var textNode = ((AtkComponentNode*)listItemNode)->Component->UldManager.NodeList[3]->GetAsAtkTextNode();
                    if (textNode == null) continue;
                    if (textNode->NodeText.ToString() != name) continue;
                    else ChangeTextColour(textNode, gold);
                }
            }
        }

        #region text colour stuff

        private  void ChangeTextColour(AtkTextNode* textNode, Vector4 colour)
        {
            textNode->TextColor.A = (byte)colour.W;
            textNode->TextColor.R = (byte)colour.X;
            textNode->TextColor.G = (byte)colour.Y;
            textNode->TextColor.B = (byte)colour.Z;
        }

        private  void ChangeAllTextColours(Vector4 colour)
        {
            var ui = GetUI();
            if (ui == null) return;
            var treeList = GetTreeList(ui);
            if (treeList == null) return;

            for (var i = 1; i <= 27; i++)
            {
                var listItemNode = treeList->AtkComponentBase.UldManager.NodeList[i];
                var textNode = ((AtkComponentNode*)listItemNode)->Component->UldManager.NodeList[3]->GetAsAtkTextNode();
                if (textNode == null) continue;
                ChangeTextColour(textNode, colour);
            }
        }

        public  void ChangeAllTextColourGrey() => ChangeAllTextColours(grey);
        public  void ResetTextColours() => ChangeAllTextColours(defaultColour);

        #endregion

        public  void PostAgendaWindowSetUp(AddonEvent _, AddonArgs __)
        {
            PluginLog.Debug("yes it is setup");
            var ui = GetUI();
            var btn = ui->GetNodeById(ScheduleButtonNodeId);
            //automatically removed
            WorkShopSearch.AddonEventManager.AddEvent((nint)ui, (nint)btn, AddonEventType.ButtonClick, ScheduleButtonClicked);
        }

        private  void ScheduleButtonClicked(AddonEventType _, IntPtr __, IntPtr ___)
        {
            PluginLog.Debug("yes the button was clicked yo");
            if (incrementalSearchTerms.Length == 0) return;
            SearchInput = string.Join(",", incrementalSearchTerms.Skip(1));
        }
        
    }
}
