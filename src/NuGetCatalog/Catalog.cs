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
        private readonly bool _reversed;

        internal Catalog(dynamic content, bool reversed)
        {
            _content = content;
            _reversed = reversed;
        }

        /// <summary>
        /// Returns all catalog pages.
        /// </summary>
        public IAsyncEnumerable<CatalogPage> Pages()
        {
            var pages = (JArray)_content.items;
            if (pages == null)
                return AsyncEnumerable.Throw<CatalogPage>(UnrecognizedJson());
            if (_reversed)
            {
                return AsyncEnumerableEx.Generate(pages.Count - 1, i => i >= 0, i => i - 1, async i =>
                {
                    var url = (string) pages[i]["@id"];
                    if (url == null)
                        throw UnrecognizedJson();
                    Console.WriteLine("Next page: " + url);
                    return new CatalogPage(await Client.GetJsonAsync(url), _reversed);
                });
            }
            else
            {
                return AsyncEnumerableEx.Generate(0, i => i < pages.Count, i => i + 1, async i =>
                {
                    var url = (string)pages[i]["@id"];
                    if (url == null)
                        throw UnrecognizedJson();
                    Console.WriteLine("Next page: " + url);
                    return new CatalogPage(await Client.GetJsonAsync(url), _reversed);
                });
            }
        }
    }
}
