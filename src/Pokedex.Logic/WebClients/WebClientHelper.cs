using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace Pokedex.Logic.WebClients
{
    public static class WebClientHelper
    {
        public static IConfiguration Configuration { get; set; }

        public static ILogger Logger { get; set; }

        public static async Task<T> GetResourceAsync<T>(string fullUri, string cacheFolder)
        {
            var (baseAddress, resource) = fullUri.SplitUrl();
            var result = await GetResourceAsync<T>(baseAddress, resource, cacheFolder);
            return result;
        }

        public static async Task<T> GetResourceAsync<T>(string baseUri, string resource, string cacheFolder)
        {
            var content = await GetCachedResource(resource, cacheFolder);
            if (string.IsNullOrEmpty(content) == false)
            {
                Logger.LogDebug("Getting cached resource: {resource}", resource);
            }
            else
            {
                Logger.LogDebug("Getting new resource: {resource}", resource);
                using (var http = new HttpClient())
                {
                    http.BaseAddress = new Uri(baseUri);
                    var response = await http.GetAsync(resource);
                    if (response.IsSuccessStatusCode == true)
                    {
                        content = await response.Content.ReadAsStringAsync();
                        await WriteCachedResource(resource, cacheFolder, content);
                    }
                }
            }

            var result = JsonConvert.DeserializeObject<T>(content);
            return result;
        }

        private static async Task<string> GetCachedResource(string resource, string cacheFolder)
        {
            var cacheFile = GetCachedFileName(resource, cacheFolder);
            if (File.Exists(cacheFile))
            {
                var content = await File.ReadAllTextAsync(cacheFile);
                return content;
            }

            return null;
        }

        private static async Task WriteCachedResource(string resource, string cacheFolder, string content)
        {
            var cacheFile = GetCachedFileName(resource, cacheFolder);

            var parentDir = Directory.GetParent(cacheFile).FullName;
            if (Directory.Exists(parentDir) == false)
                Directory.CreateDirectory(parentDir);

            await File.WriteAllTextAsync(cacheFile, content);
        }

        private static string GetCachedFileName(string resource, string cacheFolder)
        {
            while (resource.StartsWith("/") || resource.StartsWith("\\"))
                resource = resource.Substring(1);
            var cacheFile = Path.Combine(Configuration["CacheDirectory"], cacheFolder, resource);
            return cacheFile;
        }

        public static (string, string) SplitUrl(this string url)
        {
            var uri = new Uri(url);
            var baseAddress = uri.GetLeftPart(UriPartial.Authority);
            var resource = uri.AbsolutePath;

            return (baseAddress, resource);
        }

    }
}
