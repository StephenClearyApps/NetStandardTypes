using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuGetHelpers
{
    public interface IPackageContentReaderWithRef : IPackageContentReader
    {
        IEnumerable<FrameworkSpecificGroup> GetRefItems();
    }

    public static class PackageContentReaderWithRefExtensions
    {
        public static IEnumerable<NuGetFramework> GetSupportedFrameworksWithRef<T>(this T package)
            where T : PackageReaderBase, IPackageContentReaderWithRef
        {
            var frameworks = new HashSet<NuGetFramework>(new NuGetFrameworkFullComparer());
            frameworks.UnionWith(package.GetSupportedFrameworks());
            frameworks.UnionWith(package.GetRefItems().Select(x => x.TargetFramework).Where(x => !x.IsUnsupported));
            return frameworks.OrderBy(x => x, new NuGetFrameworkSorter());
        }
    }
}
