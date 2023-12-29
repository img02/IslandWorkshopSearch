using System;

namespace IslandWorkshopSearch.Windows.Service
{
    internal class GroupSearch
    {
        public static string[] GetTerms(string searchInput) => searchInput.Split(new Char[] { '|', ',', '｜', '、' });
    }
}
