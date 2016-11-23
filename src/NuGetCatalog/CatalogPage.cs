using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuGetHelpers;
using static NuGetCatalog.Globals;

namespace NuGetCatalog
{
    public sealed class CatalogPage
    {
        public CatalogPage(JToken content)
        {
            Items = content.GetNonNullValue<JArray>("items").Select(x => new Item(x)).ToList();
        }

        public IReadOnlyList<Item> Items { get; }

        public sealed class Item
        {
            private readonly JToken _content;

            internal Item(JToken content)
            {
                _content = content;
            }

            public PackageIdentity PackageIdentity => new PackageIdentity(Id, NuGetVersion.Parse(Version));

            public string Id => _content.GetNonNullValue<string>("nuget:id");
            public string Version => _content.GetNonNullValue<string>("nuget:version");
            public DateTimeOffset? CommitTimestamp => _content.GetDateTimeOffset("commitTimestamp");
            public string CommitId => _content.GetNonNullValue<string>("commitId");

            public async Task<CatalogPackage> GetPackageAsync() => new CatalogPackage(await Client.GetJsonAsync(Id));
        }
    }
}