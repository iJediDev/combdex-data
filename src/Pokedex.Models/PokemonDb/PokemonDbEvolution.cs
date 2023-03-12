namespace Pokedex.Models.PokemonDb
{
    public class PokemonDbEvolution
    {
        public readonly int BaseSpeciesNumber;

        public List<int> FamilySpeciesNumbers { get; set; } = new List<int>();

        public PokemonDbEvolution(int baseSpecies)
        {
            BaseSpeciesNumber = baseSpecies;
        }

        public override string ToString()
        {
            return string.Join(", ", FamilySpeciesNumbers);
        }
    }
}
