using CommandLine;

namespace Pokedex.Models.ProcessOptions
{
    [Verb("homesprites", HelpText = "Downloads Pokémon HOME sprites from the interwebs.")]
    public class DownloadHomeSpritesOptions
    {

        [Option('o', "out", Required = false, Default = "C:\\Projects\\Combdex-Data\\sprites\\home\\pokes", HelpText = "Directory for output files.")]
        public string OutputDirectory { get; set; }

        [Option('s', "shiny", Required = false, Default = false, HelpText = "Use this to download shiny Pokémon sprites.")]
        public bool IsShinyDex { get; set; }
    }
}
