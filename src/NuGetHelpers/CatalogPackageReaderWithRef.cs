using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;

namespace NetStandardTypes.NuGetHelpers
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
