using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace Pokedex.Logic.WebClients
{
    public static class WebClientHelper
    {
        public static IConfiguration Configuration { get; set; }

        public static ILogger Logger { get; set; }

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


        private static async Task<HttpResponseMessage> GetWebResponseAsync(string baseUri, string resource)
        {
            using(var http = new HttpClient())
            {
                http.BaseAddress = new Uri(baseUri);
                var response = await http.GetAsync(resource);
                return response;
            }
        }

        public static async Task<T> GetResourceAsync<T>(string url, string cacheFolder)
        {
            var (baseUri, resource) = SplitUrl(url);

            // Check for cache first
            var content = await GetCachedStringAsync(resource, cacheFolder);
            if(string.IsNullOrEmpty(content))
            {
                // Get new data
                Logger.LogDebug("Getting New Resource: {resource}", resource);
                var response = await GetWebResponseAsync(baseUri, resource);
                content = await response.Content.ReadAsStringAsync();

                // Write to cache
                await WriteCachedStringAsync(resource, cacheFolder, content);
            }
            else
            {
                Logger.LogDebug("Cached Resource Found: {resource}", resource);
            }

            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(content, typeof(T));

            var result = JsonConvert.DeserializeObject<T>(content);
            return result;
        }


        public static async Task<byte[]> GetResourceBytesAsync(string url, string cacheFolder)
        {
            var (baseUri, resource) = SplitUrl(url);


            // Check for cache first
            var bytes = await GetCachedBytesAsync(resource, cacheFolder);
            if(bytes == null)
            {
                // Get new data
                Logger.LogDebug("Getting New Resource: {resource}", resource);
                var response = await GetWebResponseAsync(baseUri, resource);
                bytes = await response.Content.ReadAsByteArrayAsync();

                // Write to cache
                await WriteCachedBytesAsync(resource, cacheFolder, bytes);
            }
            else
            {
                Logger.LogDebug("Cached Resource Found: {resource}", resource);
            }
            return bytes;
        }



        
        private static async Task<string> GetCachedStringAsync(string resource, string cacheFolder)
        {
            var bytes = await GetCachedBytesAsync(resource, cacheFolder);
            if (bytes == null)
                return string.Empty;

            var text = Encoding.UTF8.GetString(bytes);
            return text;
        }

        private static async Task<byte[]> GetCachedBytesAsync(string resource, string cacheFolder)
        {
            byte[] bytes = null;
            var fileName = GetCachedFileName(resource, cacheFolder);
            if(File.Exists(fileName))
                bytes = await File.ReadAllBytesAsync(fileName);
            return bytes;
        }

        private static async Task WriteCachedStringAsync(string resource, string cacheFolder, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            await WriteCachedBytesAsync(resource, cacheFolder, bytes);
        }

        private static async Task WriteCachedBytesAsync(string resource, string cacheFolder, byte[] bytes)
        {
            var fileName = GetCachedFileName(resource, cacheFolder);
            var parentDir = Directory.GetParent(fileName).FullName;

            if (Directory.Exists(parentDir) == false)
                Directory.CreateDirectory(parentDir);

            await File.WriteAllBytesAsync(fileName, bytes);
        }
    }
}
