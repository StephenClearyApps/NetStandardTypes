using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static NuGetCatalog.Globals;

namespace NuGetCatalog
{
    public sealed class ServiceIndex
    {
        private readonly dynamic _content;

        private ServiceIndex(dynamic content)
        {
            _content = content;
        }

        public static async Task<ServiceIndex> CreateAsync()
        {
            return new ServiceIndex(await Client.GetJsonAsync(V3Url).ConfigureAwait(false));
        }

        public async Task<Catalog> GetCatalogAsync()
        {
            var resources = _content.resources as IEnumerable<dynamic>;
            if (resources == null)
                throw UnrecognizedJson();
            var catalog = resources.FirstOrDefault(x => x["@type"] == "Catalog/3.0.0");
            if (catalog == null)
                throw UnrecognizedJson();
            var catalogUrl = (string)catalog["@id"];
            if (catalogUrl == null)
                throw UnrecognizedJson();
            return new Catalog(await Client.GetJsonAsync(catalogUrl).ConfigureAwait(false));
        }
    }
}
