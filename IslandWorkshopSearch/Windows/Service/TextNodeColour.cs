using FFXIVClientStructs.FFXIV.Component.GUI;
using IslandWorkshopSearch.Utility;
using System.Numerics;

namespace IslandWorkshopSearch.Windows.Service
{
    internal unsafe class TextNodeColour
    {
        private static readonly Vector4 Gold = new(255, 215, 0, 255);
        private static readonly Vector4 Grey = new(200, 200, 200, 255);
        private static readonly Vector4 White = new(255, 255, 255, 255);
        private static readonly Vector4 DefaultColour = new(235, 225, 207, 255);


        public static void ChangeTextColourGold(AtkTextNode* textNode) => ChangeTextColour(textNode, Gold);
        public static void ChangeTextColourWhite(AtkTextNode* textNode) => ChangeTextColour(textNode, White);
        public static void ChangeTextColour(AtkTextNode* textNode, Vector4 colour)
        {
            textNode->TextColor.A = (byte)colour.W;
            textNode->TextColor.R = (byte)colour.X;
            textNode->TextColor.G = (byte)colour.Y;
            textNode->TextColor.B = (byte)colour.Z;
        }


        public static void ChangeAllWorkshopTextColourGrey(AtkUnitBase* ui) => ChangeAllWorkshopTextColours(ui, Grey);
        public static void ChangeAllWorkshopTextColours(AtkUnitBase* ui, Vector4 colour)
        {
            if (ui == null) return;
            var treeList = Common.GetWorkshopAgendaTreeList(ui);
            if (treeList == null) return;

            for (var i = 1; i <= 27; i++)
            {
                var listItemNode = treeList->AtkComponentBase.UldManager.NodeList[i];
                var textNode = ((AtkComponentNode*)listItemNode)->Component->UldManager.NodeList[3]->GetAsAtkTextNode();
                if (textNode == null) continue;
                ChangeTextColour(textNode, colour);
            }
        }

        public static void ResetWorkshopTextColours(AtkUnitBase* ui) => ChangeAllWorkshopTextColours(ui, DefaultColour);
    }
}
