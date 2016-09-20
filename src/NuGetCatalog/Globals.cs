using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NuGetCatalog
{
    internal static class Globals
    {
        internal static readonly HttpClient Client = new HttpClient();

        internal static string V3Url { get; } = "https://api.nuget.org/v3/index.json";

        internal static async Task<JObject> GetJsonAsync(this HttpClient client, string url)
        {
            Trace.WriteLine("GET " + url);
            return JObject.Parse(await client.GetStringAsync(url).ConfigureAwait(false));
        }

        internal static Exception UnrecognizedJson()
        {
             return new InvalidOperationException("Unrecognized json from NuGet.");
        }
    }
}
