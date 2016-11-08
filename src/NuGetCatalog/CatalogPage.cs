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
            // Create the initial list of entries.
            var packages = (JArray)_content.items;
            if (packages == null)
                throw UnrecognizedJson();
            return packages.Select(x => new CatalogPageEntry(x));
        }
    }
}