using Pokedex.Models.LivingDex;

namespace Pokedex.Logic.Sorting
{
    public interface IBoxSorter
    {
        List<LivingDexBox> Sort(List<LivingEntry> entries);
    }
}