using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using static NuGetCatalog.Globals;

namespace NuGetCatalog
{
    public sealed class CatalogPage
    {
        private readonly dynamic _content;

        internal CatalogPage(dynamic content)
        {
            _content = content;
        }

        public IAsyncEnumerable<CatalogPackage> Packages()
        {
            var packages = (JArray)_content.items;
            if (packages == null)
                return AsyncEnumerable.Throw<CatalogPackage>(UnrecognizedJson());
            return AsyncEnumerableEx.Generate(() => packages.Count - 1, async i =>
            {
                if (i == 0)
                    return Tuple.Create(false, i, default(CatalogPackage));
                var url = (string)packages[i]["@id"];
                if (url == null)
                    throw UnrecognizedJson();
                var package = new CatalogPackage(await Client.GetJsonAsync(url));
                return Tuple.Create(true, i - 1, package);
            });
        }
    }
}