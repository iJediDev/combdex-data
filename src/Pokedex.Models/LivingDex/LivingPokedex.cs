namespace Pokedex.Models.LivingDex
{
    public class LivingPokedex
    {
        public string Identifier { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Returns a count of slots where the entry is available. IE: Not Shiny Locked.
        /// </summary>
        public int SlotCount => Boxes.Select(b => b.Slots.Count(s => s.IsAvailable)).Sum();

        public List<LivingDexBox> Boxes { get; set; } = new List<LivingDexBox>();
    }
}
