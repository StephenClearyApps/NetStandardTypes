using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using NetStandardTypes.NuGetHelpers;
using NuGet.Versioning;
using NuGetCatalog;
using NuGetHelpers;

namespace NetStandardTypes.NugetWalker
{
    public static class EntryPoint
    {
        public static async Task MainAsync()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var count = 0;

            var index = await ServiceIndex.CreateAsync();
            var catalog = await index.GetCatalogAsync();
            using (var pageEnumerator = catalog.Pages().GetEnumerator())
            {
                while (await pageEnumerator.MoveNext())
                {
                    foreach (var pageEntry in pageEnumerator.Current.Entries())
                    {
                        var key = pageEntry.Id + "@" + (NuGetVersion.Parse(pageEntry.Version).IsPrerelease ? 1 : 0);
                        if (set.Contains(key))
                            continue;
                        set.Add(key);
                        var package = await pageEntry.GetPackageAsync();
                        var id = package.Identity;
                        var frameworks = package.Metadata().GetSupportedFrameworksWithRef().ToArray();
                        if (frameworks.Any(x => new FrameworkName(x.DotNetFrameworkName).IsNetStandard()))
                        {
                            ++count;
                            Console.WriteLine(count.ToString("X") + "/" + set.Count.ToString("X") + ": " + id.Id + " " + id.Version + ", " + package.BestGuessPublicationDate + ", " + package.IsPrerelease + ", " + package.IsListed);
                        }
                    }
                }
            }

            //var packages = catalog.Pages().SelectMany(x => x.Entries());
        }
    }
}
