namespace Pokedex.Models.PokeInfo
{
    public class PokeDetail
    {
        /// <summary>
        /// {dexNum}-{form}
        /// Alcremie would be: 0869-caramel-swirl-flower
        /// Pikachu would be: 0025
        /// We'll use <see cref="LivingDex.LivingEntry.Identifier"/> to look this up.
        /// </summary>
        public string Identifier { get; set; }

        public int DexNumber { get; set; }

        public string Name { get; set; }

        public string Type1 { get; set; }

        public string Type2 { get; set; }

        /// <summary>
        /// List of {dexNum}-{form}
        /// Alcremie would be: 0869-caramel-swirl-flower
        /// Pikachu would be: 0025
        /// We'll use <see cref="LivingDex.LivingEntry.Identifier"/> to look this up.
        /// </summary>
        public List<string> Forms { get; set; }

        /// <summary>
        /// List of {dexNum}-{form}
        /// Alcremie would be: 0869-caramel-swirl-flower
        /// Pikachu would be: 0025
        /// We'll use <see cref="LivingDex.LivingEntry.Identifier"/> to look this up.
        /// </summary>
        public List<string> RelatesTo { get; set; }

        public PokeDetailReferences References { get; set; }
    }
}
