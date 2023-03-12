using Pokedex.Models.SuperEffectiveAssets;

namespace Pokedex.Logic.WebClients
{
    public interface IPokeHttpClient
    {
        Task<string> GetPokeEvolutionHtmlAsync();
        Task<List<PokemonForm>> GetPokeFormsAsync();
        Task<List<PokemonForm>> GetPokemonStorableInAsync(params GameVersion[] storeIn);
        Task<byte[]> GetSpriteSheetBytesAsync(bool isShiny = false);
        Task<Dictionary<string, (int, int)>> GetSpriteSheetCoordsAsync();
    }
}