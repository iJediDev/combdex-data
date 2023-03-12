using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pokedex.Logic.Sorting;
using Pokedex.Logic.WebClients;
using Pokedex.Models.Enums;
using Pokedex.Models.LivingDex;
using Pokedex.Models.PokeInfo;
using Pokedex.Models.PokemonDb;
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

        public async Task<LivingPokedex> GetLivingDex(LivingDexOptions options)
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

        public async Task WriteLivingDexToFile(LivingDexOptions options, LivingPokedex dex)
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



        public async Task<List<PokeDetail>> GetPokeDetailsForLivingDex(LivingPokedex livingPokedex)
        {
            var evolutions = await GetEvolutions();
            var livingDexForms = livingPokedex.Boxes.SelectMany(b => b.Slots).ToList();
            var livingDexFormIds = livingDexForms.Select(f => f.Identifier).ToList();
            var formDetails = await _pokeHttpClient.GetPokeFormsAsync();

            var results = new List<PokeDetail>();

            // Create detail for each form
            foreach(var form in livingDexForms)
            {
                if (form.IsEmpty)
                    continue;

                var result = new PokeDetail();
                result.Identifier = form.Identifier;
                result.Name = form.Name;

                var detail = formDetails.FirstOrDefault(f => f.Nid == result.Identifier);
                result.DexNumber = detail.DexNum;
                result.Type1 = detail.Type1.ToString().ToLower();
                result.Type2 = detail.Type2?.ToString().ToLower();

                // Get evolutions, excluding the current form
                var evoFamily = evolutions.Where(e => e.FamilySpeciesNumbers.Contains(detail.DexNum)).FirstOrDefault();
                if(evoFamily != null)
                {
                    var evoIds = formDetails
                    .Where(f => evoFamily.FamilySpeciesNumbers.Contains(f.DexNum))
                    .Where(f => f.DexNum != detail.DexNum && livingDexFormIds.Contains(f.Nid))
                    .Select(f => f.Nid)
                    .ToList();
                    result.RelatesTo = evoIds;
                }

                // Get forms, excluding the current form
                var forms = formDetails
                    .Where(f => f.DexNum == detail.DexNum && livingDexFormIds.Contains(f.Nid) && f.Nid != result.Identifier)
                    .Select(f => f.Nid)
                    .ToList();
                result.Forms = forms;

                result.References = new PokeDetailReferences()
                {
                    Serebii = detail.Refs?.Serebii,
                    Bulbapedia = detail.Refs?.Bulbapedia
                };

                results.Add(result);
            }


            return results;
        }

        private async Task<List<PokemonDbEvolution>> GetEvolutions()
        {
            var evoResults = new List<PokemonDbEvolution>();

            var html = await _pokeHttpClient.GetPokeEvolutionHtmlAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var evoFamily = doc.DocumentNode.Descendants().Where(n => n.HasClass("infocard-list-evo")).ToList();
            foreach (var family in evoFamily)
            {
                var infoCards = family.Descendants().Where(n => n.HasClass("infocard-lg-data")).ToList();

                PokemonDbEvolution evoResult = null;

                foreach (var card in infoCards)
                {
                    var number = int.Parse(card.FirstChild.InnerText.Replace("#", ""));

                    // Don't add duplicate families.. IE: Rattata
                    if (evoResults.Any(e => e.BaseSpeciesNumber == number))
                        evoResult = evoResults.FirstOrDefault(e => e.BaseSpeciesNumber == number);

                    // Assign base if it is a new family
                    if (evoResult == null)
                        evoResult = new PokemonDbEvolution(number);

                    // Don't add the same number twice. IE: Pikachu > Raichu forms
                    if (evoResult.FamilySpeciesNumbers.Contains(number) == false)
                        evoResult.FamilySpeciesNumbers.Add(number);
                }

                if (evoResult?.FamilySpeciesNumbers?.Any() == true
                    && evoResults.Any(e => e.BaseSpeciesNumber == evoResult.BaseSpeciesNumber) == false)
                    evoResults.Add(evoResult);
            }

            return evoResults;
        }

        public async Task WritePokeDetailsToFile(LivingDexOptions options, string dexId, List<PokeDetail> details)
        {
            // Write output
            var json = JsonConvert.SerializeObject(details);
            var parentDir = Directory.GetParent(options.OutputDirectory).FullName;
            if (Directory.Exists(parentDir) == false)
                Directory.CreateDirectory(parentDir);

            var fileName = $"{dexId}_livingdex_details.json".Replace("-", "_");
            var filePath = Path.Combine(options.OutputDirectory, fileName);

            _logger.LogInformation($"Writing file to: {filePath}");
            await File.WriteAllTextAsync(filePath, json);
        }


    }
}
