using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGetHelpers;

namespace PackageIndexer.Logic
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