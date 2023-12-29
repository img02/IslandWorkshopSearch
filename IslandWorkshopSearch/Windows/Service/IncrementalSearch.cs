using System;
using System.Collections.Generic;

namespace IslandWorkshopSearch.Windows.Service
{
    internal class IncrementalSearch
    {
        public static string[] GetTerms(string searchInput, ref string[] incrementalSearchTerms)
        {
            incrementalSearchTerms = SplitStringIncremental(searchInput);
            return new string[] { incrementalSearchTerms[0] };
        }

        private static string[] SplitStringIncremental(string searchInput)
        {
            var splitSearch = searchInput.Split(new Char[] { '|', '｜' });
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
    }
}
