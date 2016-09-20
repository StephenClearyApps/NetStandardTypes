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

        public IEnumerable<CatalogPageEntry> Entries()
        {
            var packages = (JArray)_content.items;
            if (packages == null)
                throw UnrecognizedJson();
            for (int i = packages.Count - 1; i >= 0; --i)
                yield return new CatalogPageEntry(_content.items[i]);
        }
    }
}