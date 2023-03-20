using Pokedex.Models.SuperEffectiveAssets;

namespace Pokedex.Logic.WebClients
{
    public interface IPokeHttpClient
    {
        (string, string) GetNameAndForm(PokemonForm pokemonForm);
        Task<string> GetPokeEvolutionHtmlAsync();
        Task<List<PokemonForm>> GetPokeFormsAsync();
        Task<List<PokemonForm>> GetPokemonDebutedInAsync(GameVersion storeIn);
        Task<List<PokemonForm>> GetPokemonStorableInAsync(params GameVersion[] storeIn);
        Task<byte[]> GetSpriteSheetBytesAsync(bool isShiny = false);
        Task<Dictionary<string, (int, int)>> GetSpriteSheetCoordsAsync();
    }
}