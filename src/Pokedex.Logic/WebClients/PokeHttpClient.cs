using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pokedex.Logic.Models;
using Pokedex.Models.SuperEffectiveAssets;

namespace Pokedex.Logic.WebClients
{
    public class PokeHttpClient : IPokeHttpClient
    {
        private readonly ILogger<PokeHttpClient> _logger;
        private readonly IConfiguration _config;
        private readonly PokeSpritesConfig _spritesConfig;

        private const string SuperEffectiveAssets = "SuperEffectiveAssets";
        private const string PokemonDb = "PokemonDb";

        public PokeHttpClient(ILogger<PokeHttpClient> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            _spritesConfig = _config.GetSection("PokeSprites").Get<PokeSpritesConfig>();
        }

        public async Task<List<PokemonForm>> GetPokeFormsAsync()
        {
            var results = await WebClientHelper.GetResourceAsync<List<PokemonForm>>(_config["PokeFormsURI"], SuperEffectiveAssets);
            return results;
        }

        public async Task<List<PokemonForm>> GetPokemonStorableInAsync(params GameVersion[] storeIn)
        {
            var results = await GetPokeFormsAsync();
            results = results.Where(p => p.StorableIn.Any(s => storeIn.Contains(s)) && p.IsBattleOnlyForm == false).ToList();
            return results;
        }

        public async Task<List<PokemonForm>> GetPokemonDebutedInAsync(GameVersion storeIn)
        {
            var results = await GetPokeFormsAsync();
            results = results.Where(p => p.DebutIn == storeIn && p.IsBattleOnlyForm == false).ToList();
            return results;
        }

        public async Task<Dictionary<string, (int, int)>> GetSpriteSheetCoordsAsync()
        {
            var results = new Dictionary<string, (int, int)>();
            var content = await WebClientHelper.GetResourceAsync<string>(_spritesConfig.CSSURI, SuperEffectiveAssets);

            var pkmLines = content.Split("\n").Where(c => c.StartsWith(".pkm-")).ToList();
            var row = -1;
            var col = -1;
            foreach(var line in pkmLines)
            {
                // Data: .pkm-unown, .pkm-unown-a {background-position: 79.48717948717949% 17.94871794871795%;}
                // We don't care about the Y position. We're just using the X position to determine when the row changes.
                var split = line.Split("{");
                var identifiers = split[0].Replace(".pkm-", "").Split(",").Select(id => id.Trim()).ToList();
                var values = split[1].Replace("background-position:", "").Replace(";", "").Replace("}", "").Replace("%", "").Trim().Split(" ");

                var xPer = decimal.Parse(values[0]);
                if(xPer == 0)
                {
                    row++;
                    col = 0;
                }
                else
                {
                    col++;
                }

                foreach(var id in identifiers)
                    results[id] = (row, col);
            }

            return results;
        }

        public async Task<byte[]> GetSpriteSheetBytesAsync(bool isShiny = false)
        {
            var uri = isShiny ? _spritesConfig.ShinySpriteURI : _spritesConfig.SpriteURI;
            var content = await WebClientHelper.GetResourceBytesAsync(uri, SuperEffectiveAssets);
            return content;
        }

        public async Task<string> GetPokeEvolutionHtmlAsync()
        {
            var result = await WebClientHelper.GetResourceAsync<string>(_config["PokeEvolutionsURI"], PokemonDb);
            return result;
        }

        public (string, string) GetNameAndForm(PokemonForm pokemonForm)
        {
            var name = pokemonForm.Name;
            var form = pokemonForm.FormName?.Replace("(", string.Empty).Replace(")", string.Empty);

            if (string.IsNullOrEmpty(pokemonForm.FormId) == false && string.IsNullOrEmpty(pokemonForm.FormName) == false)
            {
                // Replace with and without parenthesis
                name = name.Replace(pokemonForm.FormName, string.Empty).Replace(form, string.Empty);
            }

            var toReplace = new string[]
            {
                "(",
                ")",
                " Female",
                " Cloak",
                " Standard Mode",

                "Alolan",
                "Galarian",
                "Hisuian",
                "Paldean",
            };

            foreach (var item in toReplace)
                name = name.Replace(item, string.Empty);

            // Some names/forms need manual adjusting
            switch (pokemonForm.Id)
            {
                case "unown-u":
                    name = "Unown";
                    break;

                case "hooh":
                case "porygonz":
                    form = null;
                    break;
            }

            if (pokemonForm.Id.StartsWith("flabebe"))
                name = "Flabébé";

            name = (name ?? "").Trim();
            form = form != null ? form.Trim() : null;
            return (name, form);
        }
    }
}
