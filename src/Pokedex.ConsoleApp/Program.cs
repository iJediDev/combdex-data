using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pokedex.Logic.Processes;
using Pokedex.Logic.WebClients;
using Pokedex.Models.ProcessOptions;
using Serilog;

namespace Pokedex.ConsoleApp
{
    internal class Program
    {
        static IHost _host;

        static void Main(string[] args)
        {
            _host = BuildHost(args);

            var parser = new Parser(with =>
            {
                with.CaseInsensitiveEnumValues = true;
                with.HelpWriter = Console.Out;
            });


            parser.ParseArguments<LivingDexOptions, DownloadHomeSpritesOptions>(args)
                .WithParsed<LivingDexOptions>(RunGenerateLivingDex)
                .WithParsed<DownloadHomeSpritesOptions>(RunDownloadHomeSprites);
        }


        static IHost BuildHost(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);

            builder.ConfigureAppConfiguration(app =>
            {
                app.AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables()
                    .AddUserSecrets<Program>();
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddTransient<IPokeHttpClient, PokeHttpClient>();

                services.AddTransient<GenerateLivingDex, GenerateLivingDex>();
                services.AddTransient<DownloadHomeSprites, DownloadHomeSprites>();

                WebClientHelper.Configuration = context.Configuration;
            });

            builder.UseSerilog((context, logger) =>
            {
                logger.ReadFrom.Configuration(context.Configuration);
            });

            var host = builder.Build();

            WebClientHelper.Logger = host.Services.GetService<ILogger<Program>>();

            return host;
        }

        static void RunGenerateLivingDex(LivingDexOptions options)
        {
            var process = _host.Services.GetService<GenerateLivingDex>();
            var dex = process.Execute(options).GetAwaiter().GetResult();
            process.WriteToFile(options, dex).GetAwaiter().GetResult();
        }

        static void RunDownloadHomeSprites(DownloadHomeSpritesOptions options)
        {
            var process = _host.Services.GetService<DownloadHomeSprites>();
            process.Execute(options).GetAwaiter().GetResult();
        }


        //static void GetModels()
        //{
        //    var schema = NJsonSchema.JsonSchema.FromJsonAsync("{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"id\":{\"type\":\"string\"},\"nid\":{\"type\":\"string\",\"pattern\":\"^[0-9a-z-]+$\"},\"dexNum\":{\"type\":\"integer\"},\"formId\":{\"type\":[\"string\",\"null\"]},\"name\":{\"type\":\"string\"},\"formName\":{\"type\":[\"string\",\"null\"]},\"region\":{\"type\":\"string\"},\"generation\":{\"type\":\"integer\"},\"type1\":{\"$ref\":\"#/$defs/typeId\"},\"type2\":{\"oneOf\":[{\"$ref\":\"#/$defs/typeId\"},{\"type\":\"null\"}]},\"color\":{\"type\":[\"string\",\"null\"]},\"abilities\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"primary\":{\"type\":[\"string\",\"null\"]},\"secondary\":{\"type\":[\"string\",\"null\"]},\"hidden\":{\"type\":[\"string\",\"null\"]}},\"required\":[\"primary\",\"secondary\",\"hidden\"]},\"isLegendary\":{\"type\":\"boolean\"},\"isMythical\":{\"type\":\"boolean\"},\"isUltraBeast\":{\"type\":\"boolean\"},\"ultraBeastCode\":{\"type\":[\"string\",\"null\"]},\"isDefault\":{\"type\":\"boolean\"},\"isForm\":{\"type\":\"boolean\"},\"isSpecialAbilityForm\":{\"type\":\"boolean\"},\"isCosmeticForm\":{\"type\":\"boolean\"},\"isFemaleForm\":{\"type\":\"boolean\"},\"hasGenderDifferences\":{\"type\":\"boolean\"},\"isBattleOnlyForm\":{\"type\":\"boolean\"},\"isSwitchableForm\":{\"type\":\"boolean\"},\"isFusion\":{\"type\":\"boolean\"},\"fusedWith\":{\"type\":[\"array\",\"null\"],\"items\":{\"type\":\"array\",\"items\":{\"type\":\"string\"}}},\"isMega\":{\"type\":\"boolean\"},\"isPrimal\":{\"type\":\"boolean\"},\"isRegional\":{\"type\":\"boolean\"},\"isGmax\":{\"type\":\"boolean\"},\"canGmax\":{\"type\":\"boolean\"},\"canDynamax\":{\"type\":\"boolean\"},\"canBeAlpha\":{\"type\":\"boolean\"},\"debutIn\":{\"$ref\":\"#/$defs/gameSetId\"},\"obtainableIn\":{\"type\":\"array\",\"items\":{\"$ref\":\"#/$defs/gameSetId\"}},\"versionExclusiveIn\":{\"type\":[\"array\",\"null\"],\"items\":{\"$ref\":\"#/$defs/gameId\"}},\"eventOnlyIn\":{\"type\":\"array\",\"items\":{\"$ref\":\"#/$defs/gameSetId\"}},\"storableIn\":{\"type\":\"array\",\"items\":{\"$ref\":\"#/$defs/gameSetId\"}},\"shinyReleased\":{\"type\":\"boolean\"},\"shinyBase\":{\"type\":[\"string\",\"null\"]},\"baseStats\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"hp\":{\"type\":\"integer\"},\"atk\":{\"type\":\"integer\"},\"def\":{\"type\":\"integer\"},\"spa\":{\"type\":\"integer\"},\"spd\":{\"type\":\"integer\"},\"spe\":{\"type\":\"integer\"}},\"required\":[\"hp\",\"atk\",\"def\",\"spa\",\"spd\",\"spe\"]},\"goStats\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"atk\":{\"type\":\"integer\"},\"def\":{\"type\":\"integer\"},\"sta\":{\"type\":\"integer\"}},\"required\":[\"atk\",\"def\",\"sta\"]},\"weight\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"avg\":{\"type\":\"number\"},\"min\":{\"type\":\"number\"},\"max\":{\"type\":\"number\"},\"alpha\":{\"type\":\"number\"}},\"required\":[\"avg\",\"min\",\"max\",\"alpha\"]},\"height\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"avg\":{\"type\":\"number\"},\"min\":{\"type\":\"number\"},\"max\":{\"type\":\"number\"},\"alpha\":{\"type\":\"number\"}},\"required\":[\"avg\",\"min\",\"max\",\"alpha\"]},\"maleRate\":{\"type\":\"integer\"},\"femaleRate\":{\"type\":\"integer\"},\"refs\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"pogo\":{\"type\":[\"string\",\"null\"]},\"veekunDb\":{\"type\":[\"string\",\"null\"]},\"serebii\":{\"type\":[\"string\",\"null\"]},\"bulbapedia\":{\"type\":[\"string\",\"null\"]},\"homeSprite\":{\"type\":\"string\"},\"miniSprite\":{\"type\":\"string\"},\"showdown\":{\"type\":[\"string\",\"null\"]},\"showdownDef\":{\"type\":[\"string\",\"null\"]},\"smogon\":{\"type\":[\"string\",\"null\"]}},\"required\":[\"pogo\",\"veekunDb\",\"serebii\",\"bulbapedia\",\"homeSprite\",\"miniSprite\",\"showdown\",\"showdownDef\",\"smogon\"]},\"imgs\":{\"type\":\"object\",\"additionalProperties\":false,\"properties\":{\"home\":{\"type\":[\"string\",\"null\"]},\"menu\":{\"type\":[\"string\",\"null\"]}},\"required\":[\"home\",\"menu\"]},\"baseSpecies\":{\"type\":[\"string\",\"null\"]},\"baseForms\":{\"type\":\"array\",\"items\":{\"type\":\"string\"}},\"forms\":{\"type\":[\"array\",\"null\"],\"items\":{\"type\":\"string\"}},\"evolutions\":{\"type\":\"array\",\"items\":{\"type\":\"string\"}}},\"required\":[\"id\",\"nid\",\"dexNum\",\"formId\",\"name\",\"formName\",\"region\",\"generation\",\"type1\",\"type2\",\"color\",\"abilities\",\"isLegendary\",\"isMythical\",\"isUltraBeast\",\"ultraBeastCode\",\"isDefault\",\"isForm\",\"isSpecialAbilityForm\",\"isCosmeticForm\",\"isFemaleForm\",\"hasGenderDifferences\",\"isBattleOnlyForm\",\"isSwitchableForm\",\"isFusion\",\"fusedWith\",\"isMega\",\"isPrimal\",\"isRegional\",\"isGmax\",\"canGmax\",\"canDynamax\",\"canBeAlpha\",\"debutIn\",\"obtainableIn\",\"versionExclusiveIn\",\"eventOnlyIn\",\"storableIn\",\"shinyReleased\",\"shinyBase\",\"baseStats\",\"goStats\",\"weight\",\"height\",\"maleRate\",\"femaleRate\",\"refs\",\"baseSpecies\",\"baseForms\",\"forms\",\"evolutions\"],\"$defs\":{\"typeId\":{\"type\":\"string\",\"enum\":[\"normal\",\"fire\",\"water\",\"electric\",\"grass\",\"ice\",\"fighting\",\"poison\",\"ground\",\"flying\",\"psychic\",\"bug\",\"rock\",\"ghost\",\"dragon\",\"dark\",\"steel\",\"fairy\"]},\"gameSetId\":{\"type\":\"string\",\"enum\":[\"rb\",\"y\",\"gs\",\"c\",\"rs\",\"frlg\",\"e\",\"dp\",\"pt\",\"hgss\",\"bw\",\"b2w2\",\"xy\",\"oras\",\"go\",\"sm\",\"usum\",\"lgpe\",\"swsh\",\"home\",\"bdsp\",\"la\",\"sv\"]},\"gameId\":{\"type\":\"string\",\"enum\":[\"rb-r\",\"rb-b\",\"y\",\"gs-g\",\"gs-s\",\"c\",\"rs-r\",\"rs-s\",\"frlg-fr\",\"frlg-lg\",\"e\",\"dp-d\",\"dp-p\",\"pt\",\"hgss-hg\",\"hgss-ss\",\"bw-b\",\"bw-w\",\"b2w2-b2\",\"b2w2-w2\",\"xy-x\",\"xy-y\",\"oras-or\",\"oras-as\",\"go\",\"sm-s\",\"sm-m\",\"usum-us\",\"usum-um\",\"lgpe-p\",\"lgpe-e\",\"swsh-sw\",\"swsh-sh\",\"home\",\"bdsp-bd\",\"bdsp-sp\",\"la\",\"sv-s\",\"sv-v\"]}}}").GetAwaiter().GetResult();
        //    var generatorSettings = new CSharpGeneratorSettings();
        //    generatorSettings.GenerateDataAnnotations = true;
        //    var generator = new CSharpGenerator(schema, generatorSettings);
        //    var code = generator.GenerateFile();
        //    Debugger.Break();
        //}
    }
}