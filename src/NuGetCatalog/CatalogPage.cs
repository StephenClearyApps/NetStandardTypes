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
        private readonly bool _reversed;

        internal CatalogPage(dynamic content, bool reversed)
        {
            _content = content;
            _reversed = reversed;
        }

        public IEnumerable<CatalogPageEntry> Entries()
        {
            // Create the initial list of entries.
            var packages = (JArray)_content.items;
            if (packages == null)
                throw UnrecognizedJson();
            if (_reversed)
            {
                for (int i = packages.Count - 1; i >= 0; --i)
                    yield return new CatalogPageEntry(_content.items[i]);
            }
            else
            {
                for (int i = 0; i < packages.Count; ++i)
                    yield return new CatalogPageEntry(_content.items[i]);
            }
        }
    }
}