namespace IslandWorkshopSearch.Managers.WorkshopCrafts.Models
{
    internal class WorkshopCraftsItem
    {
        public string Name { get; }
        public uint Hours { get; }

        public WorkshopCraftsItem(string name, uint hours)
        {
            Name = name;
            Hours = hours;
        }
    }
}
