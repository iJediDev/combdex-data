using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pokedex.Logic.Models;
using Pokedex.Logic.WebClients;
using Pokedex.Models.ProcessOptions;

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

            // Get the coords for the pokemon sprites from a CSS file
            var spriteWidth = _spritesConfig.SpriteWidth;
            var spriteHeight = _spritesConfig.SpriteHeight;
            var spriteCoords = await _pokeHttpClient.GetSpriteSheetCoordsAsync();
            _logger.LogInformation("Sprites to parse: {count}", spriteCoords.Count);

            // Download the spritesheet
            var spriteBytes = await _pokeHttpClient.GetSpriteSheetBytesAsync(options.IsShinyDex);
            var sourceImage = Image.Load<Rgba32>(spriteBytes);

            // Split the source image based on the sprite coords
            // Creates a rect for the area to select and writes the data to the target image
            foreach (var coords in spriteCoords)
            {
                var identifier = coords.Key.ToLower();
                _logger.LogInformation("Parsing Sprite for: {identifier}", identifier);

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

                // Now that we have the target, we can save it 
                var outputFile = Path.Combine(options.OutputDirectory, $"{identifier}.png");
                await targetImage.SaveAsPngAsync(outputFile);
            }

        }
    }
}
