using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using NuGet.Frameworks;

namespace PackageIndexer.Logic
{
    public static class NugetExtensions
    {
        public static IEnumerable<string> GetCompatibleAssemblyReferences(this PackageArchiveReaderWithRef package, FrameworkName target)
        {
            var framework = NuGetFramework.ParseFrameworkName(target.FullName, DefaultFrameworkNameProvider.Instance);
            var result = NuGetFrameworkUtility.GetNearest(package.GetRefItems(), framework);
            if (result != null)
            {
                var items = result.Items.ToArray();
                if (items.Length != 0)
                    return items;
            }
            result = NuGetFrameworkUtility.GetNearest(package.GetLibItems(), framework);
            return result == null ? Enumerable.Empty<string>() : result.Items;
        }
    }
}
