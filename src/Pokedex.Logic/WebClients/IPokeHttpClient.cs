using Pokedex.Models.SuperEffectiveAssets;

namespace Pokedex.Logic.WebClients
{
    public interface IPokeHttpClient
    {
        Task<List<PokemonForm>> GetPokeFormsAsync();
        Task<List<PokemonForm>> GetPokemonStorableInAsync(params GameVersion[] storeIn);
    }
}