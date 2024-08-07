using FFXIVClientStructs.FFXIV.Component.GUI;
using IslandWorkshopSearch.Windows.ViewModels;

namespace IslandWorkshopSearch.Utility
{
    internal unsafe class Common
    {
        //i stole this from simpletweaks: https://github.com/Caraxi/SimpleTweaksPlugin/blob/main/Utility/Common.cs#L295
        public static AtkResNode* GetNodeByID(AtkUnitBase* unitBase, uint nodeId, NodeType? type = null) => GetNodeByID(&unitBase->UldManager, nodeId, type);
        public static AtkResNode* GetNodeByID(AtkComponentBase* component, uint nodeId, NodeType? type = null) => GetNodeByID(&component->UldManager, nodeId, type);
        public static AtkResNode* GetNodeByID(AtkUldManager* uldManager, uint nodeId, NodeType? type = null) => GetNodeByID<AtkResNode>(uldManager, nodeId, type);
        public static T* GetNodeByID<T>(AtkUldManager* uldManager, uint nodeId, NodeType? type = null) where T : unmanaged
        {
            for (var i = 0; i < uldManager->NodeListCount; i++)
            {
                var n = uldManager->NodeList[i];
                if (n->NodeId != nodeId || (type != null && n->Type != type.Value)) continue;
                return (T*)n;
            }
            return null;
        }

        public static AtkUnitBase* GetUI(string AddOnName) => (AtkUnitBase*)WorkShopSearch.GameGui!.GetAddonByName(AddOnName);

        public static AtkComponentList* GetWorkshopAgendaTreeList(AtkUnitBase* ui)
        {
            var treeList = ui->GetComponentListById(Search.TreeListNodeId);
            if (treeList == null) return null;
            if (treeList->AtkComponentBase.UldManager.NodeListCount < 27) return null;
            return treeList;
        }
    }
}
