using Pokedex.Models.Enums;

namespace Pokedex.Logic.Sorting
{
    public static class SortingHelper
    {
        public static IBoxSorter GetSorter(this BoxSortType sortType)
        {
            switch(sortType)
            {
                case BoxSortType.Default:
                    return new DefaultBoxSorter();
            }

            throw new Exception($"Could not find Box Sorter for: {sortType}");
        }
    }
}
