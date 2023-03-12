using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pokedex.Logic.Models;
using Pokedex.Logic.WebClients;
using Pokedex.Models.ProcessOptions;
using System.Text.RegularExpressions;

namespace Pokedex.Logic.Processes
{
    public class DownloadHomeSprites
    {
        private readonly ILogger<DownloadHomeSprites> _logger;
        private readonly IPokeHttpClient _pokeHttpClient;
        private readonly PokeSpritesConfig _spritesConfig;

        public DownloadHomeSprites(ILogger<DownloadHomeSprites> logger, IConfiguration config, IPokeHttpClient pokeHttpClient)
        {
            _logger = logger;
            _pokeHttpClient = pokeHttpClient;

            _spritesConfig = config.GetSection("PokeSprites").Get<PokeSpritesConfig>();
        }

        public async Task Execute(DownloadHomeSpritesOptions options)
        {
            options.OutputDirectory = Path.Combine(options.OutputDirectory, options.IsShinyDex == true ? "shiny": "normal");

            if (Directory.Exists(options.OutputDirectory) == false)
                Directory.CreateDirectory(options.OutputDirectory);

            var bigOutputDir = Path.Combine(options.OutputDirectory, "big");
            Directory.CreateDirectory(bigOutputDir);

            var smallOutputDir = Path.Combine(options.OutputDirectory, "small");
            Directory.CreateDirectory(smallOutputDir);


            // Get the coords for the pokemon sprites from a CSS file
            var spriteWidth = _spritesConfig.SpriteWidth;
            var spriteHeight = _spritesConfig.SpriteHeight;
            var spriteCoords = await _pokeHttpClient.GetSpriteSheetCoordsAsync();
            _logger.LogInformation("Sprites to parse: {count}", spriteCoords.Count);

            // Download the spritesheet
            var spriteBytes = await _pokeHttpClient.GetSpriteSheetBytesAsync(options.IsShinyDex);
            var sourceImage = Image.Load<Rgba32>(spriteBytes);

            // Get form info
            var forms = await _pokeHttpClient.GetPokeFormsAsync();

            // Split the source image based on the sprite coords
            // Creates a rect for the area to select and writes the data to the target image
            foreach (var coords in spriteCoords)
            {
                var formIdentifier = coords.Key.ToLower();

                // Find the national id. IE: 0001, 0002, 0003-f
                var spriteIdentifier = forms.FirstOrDefault(f => f.Id == formIdentifier)?.Nid;
                if(string.IsNullOrEmpty(spriteIdentifier) == true)
                    spriteIdentifier = forms.FirstOrDefault(f => $"{f.Id}-{f.FormId}" == formIdentifier)?.Nid;

                // Some unreleased pokes may not have a national id, so they will have a value of 0000
                // Clear that value so that they will be populated with their form name instead. IE: 0000 -> terapagos
                if(string.IsNullOrEmpty(spriteIdentifier) == false)
                {
                    var splitId = spriteIdentifier.Split('-');
                    if (int.TryParse(splitId[0], out var nationalId) == true && nationalId == 0)
                        spriteIdentifier = string.Empty;
                }

                if (string.IsNullOrEmpty(spriteIdentifier) == true)
                {
                    _logger.LogWarning("Could not find Form Nid for: {formIdentifier}", formIdentifier);
                    spriteIdentifier = formIdentifier;
                }





                _logger.LogDebug("Parsing Sprite for: {formIdentifier}", formIdentifier);

                var (row, col) = coords.Value;
                var sourceArea = new Rectangle(spriteWidth * col, spriteHeight * row, spriteWidth, spriteHeight);
                var targetImage = new Image<Rgba32>(sourceArea.Width, sourceArea.Height);

                sourceImage.ProcessPixelRows(targetImage, (sourceAccessor, targetAccessor) =>
                {
                    for (var i = 0; i < sourceArea.Height; i++)
                    {
                        Span<Rgba32> sourceRow = sourceAccessor.GetRowSpan(sourceArea.Y + i);
                        Span<Rgba32> targetRow = targetAccessor.GetRowSpan(i);

                        sourceRow.Slice(sourceArea.X, sourceArea.Width).CopyTo(targetRow);
                    }
                });

                // Write large version first
                var outputFile = Path.Combine(bigOutputDir, $"{spriteIdentifier}.png");
                await targetImage.SaveAsPngAsync(outputFile);


                // Shrink the image
                var resizeOptions = new ResizeOptions()
                {
                    Size = new Size(_spritesConfig.SpriteResizeWidth, _spritesConfig.SpriteResizeHeight),
                    Sampler = KnownResamplers.Lanczos8,
                    Compand = true,
                    Mode = ResizeMode.Stretch
                };
                targetImage.Mutate(image => image.Resize(resizeOptions));

                // Now that we have the target, we can save it 
                outputFile = Path.Combine(smallOutputDir, $"{spriteIdentifier}.png");
                await targetImage.SaveAsPngAsync(outputFile);
            }

        }
    }
}
