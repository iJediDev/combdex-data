using CommandLine;
using Pokedex.Models.Enums;
using Pokedex.Models.SuperEffectiveAssets;

namespace Pokedex.Models.ProcessOptions
{
    [Verb("living", HelpText = "Generate a complete list of Pokémon that can be stored in the given game version.")]
    public class LivingDexOptions
    {
        [Option('o', "out", Required = false, Default = "C:\\Projects\\Combdex-Data\\data", HelpText = "Directory for output files.")]
        public string OutputDirectory { get; set; }

        [Option('v', "version", Required = true, HelpText = "The version of the game that the Pokémon can be stored in.")]
        public GameVersion GameVersion { get; set; }

        [Option('s', "shiny", Required = false, Default = false, HelpText = "Use this to filter out shiny locked Pokémon.")]
        public bool IsShinyDex { get; set; }

        [Option("uniquealcremie", Required = false, Default = false, HelpText = "Use this to filter out shiny Alcremie forms. They are different entries, but appear the same.")]
        public bool IsUniqueAlcremie { get; set; }

        [Option("sort", Required = false, Default = BoxSortType.Default, HelpText = "Use this to change the sorting.")]
        public BoxSortType SortType { get; set; }

        [Option('n', "name", Required = true, HelpText = "Name of the Pokédex.")]
        public string DexName { get; set; }
    }
}
