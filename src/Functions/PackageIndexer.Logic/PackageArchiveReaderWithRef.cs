using System.Collections.Generic;
using System.IO;
using NuGet.Packaging;
using NuGetHelpers;

namespace NetStandardTypes.PackageIndexer
{
    public sealed class PackageArchiveReaderWithRef : PackageArchiveReader, IPackageContentReaderWithRef
    {
        public PackageArchiveReaderWithRef(Stream stream, bool leaveStreamOpen)
            : base(stream, leaveStreamOpen)
        {
        }

        public IEnumerable<FrameworkSpecificGroup> GetRefItems()
        {
            return GetFileGroups(PackagingConstants.Folders.Ref);
        }
    }
}