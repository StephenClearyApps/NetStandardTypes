using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;
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

        public PackageIdentity PackageIdentity => new PackageIdentity(Id, NuGetVersion.Parse(Version));

        public string Id => (string) _content["nuget:id"];
        public string Version => (string)_content["nuget:version"];

        public async Task<CatalogPackage> GetPackageAsync()
        {
            var url = (string)_content["@id"];
            if (url == null)
                throw UnrecognizedJson();
            return new CatalogPackage(await Client.GetJsonAsync(url));
        }
    }
}