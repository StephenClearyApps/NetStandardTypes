using System.Collections.Generic;
using System.Linq;
using NuGet.Frameworks;
using NuGet.Packaging;

namespace NetStandardTypes.NuGetHelpers
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
