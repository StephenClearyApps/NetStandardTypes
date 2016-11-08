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
                        // Ensure the version is parseable.
                        if (!IsVersionValid(pageEntry.Version))
                        {
                            log.WriteLine("Unable to parse version for " + pageEntry.Id + " " + pageEntry.Version);
                            continue;
                        }

                        // Look up the existing entry, if any.
                        var existing = await table.TryGetVersionAsync(pageEntry.Id, pageEntry.Version);
                        if (string.Equals(existing, pageEntry.Id, StringComparison.InvariantCultureIgnoreCase))
                        {
                            log.WriteLine("Reached bookmark " + pageEntry.Id + " " + pageEntry.Version);
                            return;
                        }

                        // Ensure the package supports netstandard.
                        var package = await pageEntry.GetPackageAsync();
                        var id = package.Identity;
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

        private static bool IsVersionValid(string version)
        {
            NuGetVersion _;
            return NuGetVersion.TryParse(version, out _);
        }
    }
}
