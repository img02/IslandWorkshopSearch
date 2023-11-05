using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using IslandWorkshopSearch.Managers.WorkshopCrafts;
using IslandWorkshopSearch.Managers.WorkshopCrafts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Addon.Events;
using System.IO;

namespace IslandWorkshopSearch.Windows.ViewModels
{
    internal unsafe class Search
    {
        public static float Scale => UpdateScale();

        public const string AddOnName = "MJICraftScheduleSetting";
        private const uint TreeListNodeId = 7;
        private const uint ScheduleButtonNodeId = 55;

        public static string SearchInput = string.Empty;      
        private static string[] IncrementalSearchTerms = Array.Empty<string>();       
        private static readonly List<WorkshopCraftsItem> Crafts = WorkshopCrafts.GetWorkshopItemsList();
                
        private static readonly Vector4 Gold = new(255, 215, 0, 255);
        private static readonly Vector4 Grey = new(200, 200, 200, 255);
        private static readonly Vector4 White = new(255, 255, 255, 255);
        private static readonly Vector4 DefaultColour = new(235, 225, 207, 255);

        public static void UpdateSearch(string searchInput) => SearchInput = searchInput;

        #region atk stuff

        public static AtkUnitBase* GetUI() => (AtkUnitBase*)WorkShopSearch.GameGui.GetAddonByName(AddOnName);
        public static bool UiExists() => GetUI() != null;
        private static AtkComponentList* GetTreeList(AtkUnitBase* ui)
        {
            var treeList = ui->GetComponentListById(TreeListNodeId);
            if (treeList == null) return null;
            if (treeList->AtkComponentBase.UldManager.NodeListCount < 27) return null;
            return treeList;
        }

        #endregion

        private static float UpdateScale()
        {
            var ui = GetUI();
            return ui == null ? 1f : ui->Scale * ImGuiHelpers.GlobalScale;
        }

        public static Vector2 GetWorkshopAgendaGuiWindowPosWowThisIsAReallyLongName()
        {
            var ui = GetUI();
            if (ui == null) return Vector2.Zero;
            return new Vector2(ui->GetX() + (Scale * 26), ui->GetY() - (Scale * 20));
        }

        public static void SearchWorkshop()
        {
            if (SearchInput.Length == 0)
            {
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

        private static void HighlightTabs(bool four, bool six, bool eight)
        {
            var ui = GetUI();
            if (ui == null) return;
            var treeList = GetTreeList(ui);
            if (treeList == null) return;

            if (four)
            {
                var fourHourList = treeList->AtkComponentBase.UldManager.NodeList[28];
                var fourText = ((AtkComponentNode*)fourHourList)->Component->UldManager.NodeList[2]->GetAsAtkTextNode();
                ChangeTextColour(fourText, Gold);
            }
            if (six)
            {
                var sixHourList = treeList->AtkComponentBase.UldManager.NodeList[29];
                var sixText = ((AtkComponentNode*)sixHourList)->Component->UldManager.NodeList[2]->GetAsAtkTextNode();
                ChangeTextColour(sixText, Gold);
            }
            if (eight)
            {
                var eightHourList = treeList->AtkComponentBase.UldManager.NodeList[30];
                var eightText = ((AtkComponentNode*)eightHourList)->Component->UldManager.NodeList[2]->GetAsAtkTextNode();
                ChangeTextColour(eightText, Gold);
            }
        }

        private static void ResetTabColours()
        {
            var ui = GetUI();
            if (ui == null) return;
            var treeList = GetTreeList(ui);
            if (treeList == null) return;

            for (var i = 28; i <= 30; i++)
            {
                var list = treeList->AtkComponentBase.UldManager.NodeList[i];
                var text = ((AtkComponentNode*)list)->Component->UldManager.NodeList[2]->GetAsAtkTextNode();
                ChangeTextColour(text, White);
            }
        }

        #region search  string filtering
        private static string[] GetSearchTerms()
        {
            // if it's got a pipe - grouped search, empty out incremental search terms
            if (SearchInput.Contains('|') || SearchInput.Contains('｜'))
            {
                //PluginLog.Debug("grouped search : " + SearchInput);
                IncrementalSearchTerms = Array.Empty<string>();
                return GroupSearch();
            }
            //PluginLog.Debug("step-by-step search");
            return IncrementalSearch();
        }

        private static string[] IncrementalSearch()
        {
            IncrementalSearchTerms = SplitStringIncremental();
            return new string[] { IncrementalSearchTerms[0] };
        }

        private static string[] SplitStringIncremental()
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

        private static string[] GroupSearch() => SearchInput.Split(new Char[] { '|', ',', '｜', '、' });

        #endregion

        private static string[] MatchItemsAndFilterHours(string[] toMatch, ref bool four, ref bool six, ref bool eight)
        {
            var searchedFor = new List<string>();
            //PluginLog.Debug("----------------------");            

            foreach (var name in toMatch)
            {
                if (name.Length == 0) continue;
                foreach (var c in Crafts.Where(c => c.Name.ToLowerInvariant().Contains(name.Trim().ToLowerInvariant())))
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

        public static void FilterWorkshopItemList(string[] toSearch)
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
                    else ChangeTextColour(textNode, Gold);
                }
            }
        }

        #region text colour stuff

        private static void ChangeTextColour(AtkTextNode* textNode, Vector4 colour)
        {
            textNode->TextColor.A = (byte)colour.W;
            textNode->TextColor.R = (byte)colour.X;
            textNode->TextColor.G = (byte)colour.Y;
            textNode->TextColor.B = (byte)colour.Z;
        }

        private static void ChangeAllTextColours(Vector4 colour)
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

        public static void ChangeAllTextColourGrey() => ChangeAllTextColours(Grey);
        public static void ResetTextColours() => ChangeAllTextColours(DefaultColour);

        #endregion

        public static void PostAgendaWindowSetUp(AddonEvent type, AddonArgs args)
        {
            PluginLog.Debug("yes it is setup");
            var ui = GetUI();            
            var btn = ui->GetNodeById(ScheduleButtonNodeId);
            //automatically removed
            WorkShopSearch.AddonEventManager.AddEvent((nint)ui, (nint)btn, AddonEventType.ButtonClick, ScheduleButtonClicked);
        }

        private static void ScheduleButtonClicked(AddonEventType atkEventType, IntPtr atkUnitBase, IntPtr atkResNode)
        {
            PluginLog.Debug("yes the button was clicked yo");
            if (IncrementalSearchTerms.Length == 0) return;
            SearchInput = string.Join(",", IncrementalSearchTerms.Skip(1));            
        }

        #region none of this panned out, but I don't want to delete it
        //this is jank af
        public static void MoveHoursTabsTop()
        {
            var initPos = 0;
            for (var i = 28; i <= 30; i++)
            {
                var ui = GetUI();
                if (ui == null) return;
                var treeList = ui->GetComponentListById(TreeListNodeId);
                var listItemNode = treeList->AtkComponentBase.UldManager.NodeList[i];
                var textNode = ((AtkComponentNode*)listItemNode)->Component->UldManager.NodeList[2]->GetAsAtkTextNode();
                if (textNode == null) continue;

                if (!textNode->NodeText.ToString().ToLowerInvariant().Contains(" Hours"))
                {
                    listItemNode->SetY(0);
                    listItemNode->SetX(initPos);
                    listItemNode->SetWidth(100);
                    initPos += 100;

                    for (var j = 0; j < 2; j++)
                    {
                        var node = ((AtkComponentNode*)listItemNode)->Component->UldManager.NodeList[j];
                        node->SetWidth(100);
                    }
                }
            }
        }

        // not currently used, jank
        public static void HideHoursTabs()
        {
            for (var i = 28; i <= 30; i++)
            {
                var ui = GetUI();
                if (ui == null) return;
                var treeList = ui->GetComponentListById(TreeListNodeId);
                var listItemNode = treeList->AtkComponentBase.UldManager.NodeList[i];
                var textNode = ((AtkComponentNode*)listItemNode)->Component->UldManager.NodeList[2]->GetAsAtkTextNode();
                if (textNode == null) continue;

                if (!textNode->NodeText.ToString().ToLowerInvariant().Contains(" Hours")) listItemNode->SetY(-50);
            }
        }

        public static void HideScrollBar()
        {
            var ui = GetUI();
            if (ui == null) return;
            var treeList = ui->GetComponentListById(TreeListNodeId);
            var listItemNode = treeList->AtkComponentBase.UldManager.NodeList[48];
            listItemNode->ToggleVisibility(false);
        }

        public void HideSortByBox()
        {
            var ui = GetUI();
            if (ui == null) return;
            ui->UldManager.NodeList[53]->ToggleVisibility(false);
        }
        #endregion
    }
}
