using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static NuGetCatalog.Globals;

namespace NuGetCatalog
{
    public sealed class Catalog
    {
        private readonly dynamic _content;

        internal Catalog(dynamic content)
        {
            _content = content;
        }

        /// <summary>
        /// Returns all catalog pages, from newest to oldest.
        /// </summary>
        public IAsyncEnumerable<CatalogPage> Pages()
        {
            var pages = (JArray)_content.items;
            if (pages == null)
                return AsyncEnumerable.Throw<CatalogPage>(UnrecognizedJson());
            return AsyncEnumerableEx.Generate(() => pages.Count - 1, async i =>
            {
                if (i == 0)
                    return Tuple.Create(false, i, default(CatalogPage));
                var url = (string)pages[i]["@id"];
                if (url == null)
                    throw UnrecognizedJson();
                var page = new CatalogPage(await Client.GetJsonAsync(url));
                return Tuple.Create(true, i - 1, page);
            });
        }
    }
}
