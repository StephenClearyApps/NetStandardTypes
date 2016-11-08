using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using NetStandardTypes.NuGetHelpers;
using NuGet.Versioning;
using NuGetCatalog;
using NuGetHelpers;

namespace NetStandardTypes.NugetWalker
{
    public static class EntryPoint
    {
        private static readonly Task _initialize = PackageTable.InitializeAsync();

        public static async Task Run(RefreshCatalogRequest request, IAsyncCollector<IndexPackageRequest> processPackageQueue, TextWriter log)
        {
            await _initialize;
            var table = new PackageTable();

            var index = await ServiceIndex.CreateAsync();
            var catalog = await index.GetCatalogAsync();
            using (var pageEnumerator = catalog.Pages().GetEnumerator())
            {
                while (await pageEnumerator.MoveNext())
                {
                    foreach (var pageEntry in pageEnumerator.Current.Entries())
                    {
                        // Parse the version.
                        NuGetVersion version;
                        if (!NuGetVersion.TryParse(pageEntry.Version, out version))
                        {
                            log.WriteLine("Unable to parse version for " + pageEntry.Id + " " + pageEntry.Version);
                            continue;
                        }

                        // Look up the existing entry, if any.
                        var existing = await table.TryGetVersionAsync(pageEntry.Id, pageEntry.Version);

                        // If the existing entry is this same one, then we're done.
                        if (string.Equals(existing, pageEntry.Version, StringComparison.InvariantCultureIgnoreCase))
                        {
                            log.WriteLine("Reached bookmark " + pageEntry.Id + " " + pageEntry.Version);
                            return;
                        }

                        // If the existing entry is newer than this one, then skip it.
                        //  (This can only happen if NuGet packages are published out of order. Which does seem to happen!)
                        if (existing != null && version < NuGetVersion.Parse(existing))
                        {
                            log.WriteLine("Ignoring " + pageEntry.Id + " " + pageEntry.Version + " because version " + existing + " is already listed.");
                            continue;
                        }

                        // Ensure the package supports netstandard.
                        var package = await pageEntry.GetPackageAsync();
                        var frameworks = package.Metadata().GetSupportedFrameworksWithRef().ToArray();
                        if (!frameworks.Any(x => new FrameworkName(x.DotNetFrameworkName).IsNetStandard()))
                        {
                            log.WriteLine("Ignoring " + pageEntry.Id + " " + pageEntry.Version + " because it does not support netstandard.");
                            continue;
                        }

                        // Add a process request.
                        await processPackageQueue.AddAsync(new IndexPackageRequest
                        {
                            PackageId = pageEntry.Id,
                            PackageVersion = pageEntry.Version,
                        });
                        //Console.WriteLine(count.ToString("X") + "/" + set.Count.ToString("X") + ": " + id.Id + " " + id.Version + ", " + package.BestGuessPublicationDate + ", " + package.IsPrerelease + ", " + package.IsListed);
                    }
                }
            }
        }
    }
}
