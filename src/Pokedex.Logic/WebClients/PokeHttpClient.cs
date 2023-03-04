using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pokedex.Models.SuperEffectiveAssets;

namespace Pokedex.Logic.WebClients
{
    public class PokeHttpClient : IPokeHttpClient
    {
        private readonly ILogger<PokeHttpClient> _logger;
        private readonly IConfiguration _config;

        private const string SuperEffectiveAssets = "SuperEffectiveAssets";

        public PokeHttpClient(ILogger<PokeHttpClient> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<List<PokemonForm>> GetPokeFormsAsync()
        {
            var results = await WebClientHelper.GetResourceAsync<List<PokemonForm>>(_config["PokeFormsURI"], SuperEffectiveAssets);
            return results;
        }

        public async Task<List<PokemonForm>> GetPokemonStorableInAsync(params GameVersion[] storeIn)
        {
            var results = await GetPokeFormsAsync();
            results = results.Where(p => p.StorableIn.Any(s => storeIn.Contains(s))).ToList();
            return results;
        }
    }
}
