using System.Collections.Generic;

namespace NuGetCatalog
{
    public sealed class CatalogPage
    {
        private readonly dynamic _content;

        internal CatalogPage(dynamic content)
        {
            _content = content;
        }
    }
}