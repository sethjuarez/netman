using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace netman
{
    public class Client
    {
        private static readonly string BaseUrl = "http://netman.io";
        public static void Download(Package package)
        {
            var src = $"{package.Remote}/{package.Name}.zip";
            var destination = Path.Combine(Package.GetPackagePath(), $"{package.Name}.zip");
            WebClient wc = new WebClient();
            wc.DownloadFile(src, destination);
        }

        public static async Task<Package[]> GetPackagesAsync(string name)
        {
            Uri manifest = new Uri($"{BaseUrl}/{name}/manifest.json");

            WebClient wc = new WebClient();
            var json = await wc.DownloadStringTaskAsync(manifest);
            return GetPackagesFromJson(json);
        }

        public static Package[] GetPackages(string name)
        {
            Uri manifest = new Uri($"{BaseUrl}/{name}/manifest.json");
            WebClient wc = new WebClient();
            var json = wc.DownloadString(manifest);
            return GetPackagesFromJson(json);
        }

        public static Package[] GetPackagesFromJson(string json)
        {
            return JsonConvert.DeserializeObject<Package[]>(json);
        }
    }
}
