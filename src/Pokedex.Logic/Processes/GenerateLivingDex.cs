using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pokedex.Logic.Sorting;
using Pokedex.Logic.WebClients;
using Pokedex.Models.Enums;
using Pokedex.Models.LivingDex;
using Pokedex.Models.ProcessOptions;

namespace Pokedex.Logic.Processes
{
    public class GenerateLivingDex
    {
        private readonly ILogger _logger;
        private readonly IPokeHttpClient _pokeHttpClient;

        public GenerateLivingDex(ILogger<GenerateLivingDex> logger, IPokeHttpClient pokeHttpClient)
        {
            _logger = logger;
            _pokeHttpClient = pokeHttpClient;
        }

        public async Task<LivingPokedex> Execute(LivingDexOptions options)
        {
            var forms = await _pokeHttpClient.GetPokemonStorableInAsync(options.GameVersion);

            if(options.IsUniqueAlcremie)
            {
                // Filter out duplicate Alcremie forms. The shiny versions appear the same.

                // Find the first form of Alcremie
                // IE: vanilla-cream-strawberry => vanilla-cream
                var defaultForm = forms.Where(f => f.DexNum == (int)Species.Alcremie).FirstOrDefault();
                if(defaultForm != null)
                {
                    var defaultFormId = defaultForm.FormId.Substring(0, defaultForm.FormId.LastIndexOf('-'));

                    // Remove all Alcremie forms that aren't the default vanilla-cream
                    forms.RemoveAll(f => f.DexNum == defaultForm.DexNum && f.FormId.StartsWith(defaultFormId) == false);

                    // TODO: Fix Alcremie names
                    // IE: Alcremie (Caramel Swirl Flower) => Alcremie (Flower)
                }
            }


            // Convert to living entry
            var livingEntries = new List<LivingEntry>();
            foreach(var form in forms)
            {
                var entry = new LivingEntry(form);
                entry.IsShiny = options.IsShinyDex;
                livingEntries.Add(entry);
            }

            _logger.LogInformation($"Forms found: {forms.Count}");
            _logger.LogInformation($"Forms available: {livingEntries.Count(e => e.IsAvailable)}");


            // Apply sort
            var sorter = options.SortType.GetSorter();
            var boxes = sorter.Sort(livingEntries);

            // Build dex
            var dex = new LivingPokedex();
            dex.Name = options.DexName;
            dex.Identifier = options.DexName.ToLower().Replace(" ", "-");
            dex.Boxes = boxes;

            return dex;
        }

        public async Task WriteToFile(LivingDexOptions options, LivingPokedex dex)
        {
            // Write output
            var json = JsonConvert.SerializeObject(dex);
            var parentDir = Directory.GetParent(options.OutputDirectory).FullName;
            if (Directory.Exists(parentDir) == false)
                Directory.CreateDirectory(parentDir);

            var fileName = $"{dex.Identifier}_livingdex.json".Replace("-", "_");
            var filePath = Path.Combine(options.OutputDirectory, fileName);

            _logger.LogInformation($"Writing file to: {filePath}");
            await File.WriteAllTextAsync(filePath, json);
        }
    }
}
