using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static NuGetCatalog.Globals;

namespace NuGetCatalog
{
    public sealed class CatalogPageEntry
    {
        private readonly JToken _content;

        internal CatalogPageEntry(dynamic content)
        {
            _content = content;
        }

        public string Id => (string) _content["nuget:id"];

        public async Task<CatalogPackage> GetPackageAsync()
        {
            var url = (string)_content["@id"];
            if (url == null)
                throw UnrecognizedJson();
            return new CatalogPackage(await Client.GetJsonAsync(url));
        }
    }
}