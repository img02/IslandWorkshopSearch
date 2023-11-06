namespace IslandWorkshopSearch.Managers.WorkshopCrafts.Models
{
    internal class WorkshopCraftsItem
    {
        public string Name { get; }
        public uint Hours { get; }
        public uint ID { get; }

        public WorkshopCraftsItem(string name, uint hours, uint iD)
        {
            Name = name;
            Hours = hours;
            ID = iD;
        }
    }
}
