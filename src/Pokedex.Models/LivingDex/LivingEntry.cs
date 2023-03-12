using Newtonsoft.Json;
using Pokedex.Models.Enums;
using Pokedex.Models.SuperEffectiveAssets;

namespace Pokedex.Models.LivingDex
{
    public class LivingEntry
    {
        [JsonIgnore]
        public readonly PokemonForm Form; 

        /// <summary>
        /// {dexNum}-{form}
        /// Alcremie would be: 0869-caramel-swirl-flower
        /// Pikachu would be: 0025
        /// </summary>
        public string Identifier => Form?.Nid;

        public string Name => Form?.Name;

        public bool IsFemaleForm => Form?.IsFemaleForm ?? false;

        public bool IsShiny { get; set; }

        public bool IsAvailable
        {
            get
            {
                if (Form == null)
                    return false;

                return IsShiny == false || (IsShiny == true && Form?.ShinyReleased == true);
            }
        }

        public bool IsEmpty => Form == null;

        [JsonIgnore]
        public bool IsPikachuCap => Form?.DexNum == (int)Species.Pikachu && Form?.IsFemaleForm == false && Form?.IsForm == true;

        [JsonIgnore]
        public bool IsRegionalForm => Form?.IsRegional == true && IsPikachuCap == false;

        [JsonIgnore]
        public int DexNumber => Form?.DexNum ?? -1;

        public LivingEntry(PokemonForm form)
        {
            this.Form = form;
        }

        public override string ToString()
        {
            return $"{Name} | {Identifier} | Available: {IsAvailable}";
        }
    }
}
