using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NuGetHelpers;
using static NuGetCatalog.Globals;

namespace NuGetCatalog
{
    public sealed class Catalog
    {
        public Catalog(JToken content)
        {
            Items = content.GetNonNullValue<JArray>("items").Select(x => new Item(x)).ToList();
        }

        public IReadOnlyList<Item> Items { get; }

        public sealed class Item
        {
            private readonly JToken _content;

            public Item(JToken content)
            {
                _content = content;
            }

            public string Id => _content.GetNonNullValue<string>("@id");

            public async Task<CatalogPage> GetCatalogPageAsync() => new CatalogPage(await Client.GetJsonAsync(Id));
        }
    }
}
