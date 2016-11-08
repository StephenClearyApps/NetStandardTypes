using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Polly;

namespace NuGetCatalog
{
    internal static class Globals
    {
        internal static readonly HttpClient Client = new HttpClient();

        internal static string V3Url { get; } = "https://api.nuget.org/v3/index.json";

        internal static async Task<JObject> GetJsonAsync(this HttpClient client, string url)
        {
            Trace.WriteLine("GET " + url);
            var json = await Policy.Handle<Exception>()
                .WaitAndRetryAsync(20, count => TimeSpan.FromSeconds(count))
                .ExecuteAsync(() => client.GetStringAsync(url)).ConfigureAwait(false);
            return JObject.Parse(json);
        }

        internal static Exception UnrecognizedJson()
        {
             return new InvalidOperationException("Unrecognized json from NuGet.");
        }
    }
}
