using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet.Frameworks;
using NuGet.Packaging;

namespace PackageIndexer.Logic
{
    public sealed class PackageArchiveReaderWithRef : PackageArchiveReader
    {
        public PackageArchiveReaderWithRef(Stream stream, bool leaveStreamOpen)
            : base(stream, leaveStreamOpen)
        {
        }

        public IEnumerable<FrameworkSpecificGroup> GetRefItems()
        {
            return GetFileGroups(PackagingConstants.Folders.Ref);
        }

        public new IEnumerable<NuGetFramework> GetSupportedFrameworks()
        {
            var frameworks = new HashSet<NuGetFramework>(new NuGetFrameworkFullComparer());
            frameworks.UnionWith(base.GetSupportedFrameworks());
            frameworks.UnionWith(GetRefItems().Select(x => x.TargetFramework));
            return frameworks.OrderBy(x => x, new NuGetFrameworkSorter());
        }
    }
}