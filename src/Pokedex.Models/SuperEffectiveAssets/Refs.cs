namespace Pokedex.Models.SuperEffectiveAssets
{
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.8.0.0 (Newtonsoft.Json v9.0.0.0)")]
    public partial class Refs
    {
        [Newtonsoft.Json.JsonProperty("pogo", Required = Newtonsoft.Json.Required.AllowNull)]
        public string Pogo { get; set; }

        [Newtonsoft.Json.JsonProperty("veekunDb", Required = Newtonsoft.Json.Required.AllowNull)]
        public string VeekunDb { get; set; }

        [Newtonsoft.Json.JsonProperty("serebii", Required = Newtonsoft.Json.Required.AllowNull)]
        public string Serebii { get; set; }

        [Newtonsoft.Json.JsonProperty("bulbapedia", Required = Newtonsoft.Json.Required.AllowNull)]
        public string Bulbapedia { get; set; }

        [Newtonsoft.Json.JsonProperty("homeSprite", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string HomeSprite { get; set; }

        [Newtonsoft.Json.JsonProperty("miniSprite", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string MiniSprite { get; set; }

        [Newtonsoft.Json.JsonProperty("showdown", Required = Newtonsoft.Json.Required.AllowNull)]
        public string Showdown { get; set; }

        [Newtonsoft.Json.JsonProperty("showdownDef", Required = Newtonsoft.Json.Required.AllowNull)]
        public string ShowdownDef { get; set; }

        [Newtonsoft.Json.JsonProperty("smogon", Required = Newtonsoft.Json.Required.AllowNull)]
        public string Smogon { get; set; }


    }
}
