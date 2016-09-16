using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Packaging;

namespace NuGetHelpers
{
    public sealed class CatalogPackageReaderWithRef : CatalogPackageReader, IPackageContentReaderWithRef
    {
        public CatalogPackageReaderWithRef(JObject catalogItem)
            : base(catalogItem)
        {
        }

        public IEnumerable<FrameworkSpecificGroup> GetRefItems()
        {
            return GetFileGroups(PackagingConstants.Folders.Ref);
        }
    }
}
