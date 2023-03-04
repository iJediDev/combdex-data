using Newtonsoft.Json;

namespace Pokedex.Models.LivingDex
{
    public class LivingDexBox
    {
        public int BoxNumber { get; set; }

        public List<LivingEntry> Slots { get; set; } = new List<LivingEntry>();

        [JsonIgnore]
        public int SlotCount { get; set; }

        [JsonIgnore]
        public bool IsFull => Slots.Count >= SlotCount;

        public override string ToString()
        {
            var first = Slots.FirstOrDefault()?.ToString();
            var last = Slots.LastOrDefault()?.ToString();

            return $"First: {first} | Last: {last}";
        }
    }
}
