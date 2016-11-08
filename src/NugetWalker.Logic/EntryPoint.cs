using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using NetStandardTypes.NuGetHelpers;
using Nito.Comparers;
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
                    foreach (var pageEntry in Fixup(pageEnumerator.Current.Entries()))
                    {
                        // Parse the version.
                        NuGetVersion version;
                        if (!NuGetVersion.TryParse(pageEntry.Version, out version))
                        {
                            log.WriteLine("Unable to parse version: " + pageEntry.Id + " " + pageEntry.Version);
                            continue;
                        }

                        // Look up the existing entry, if any.
                        var existing = await table.TryGetVersionCommitAsync(pageEntry.Id, pageEntry.Version);

                        // If the existing entry is this same one, then we're done.
                        if (existing != null && string.Equals(existing.CommitId, pageEntry.CommitId, StringComparison.InvariantCultureIgnoreCase))
                        {
                            log.WriteLine("Reached bookmark: " + pageEntry.Id + " " + pageEntry.Version + " at " + pageEntry.CommitId);
                            return;
                        }

                        // If the existing entry is newer than this one, then skip it.
                        //  (This can only happen if NuGet packages are published out of order. Which *does* seem to happen!)
                        if (existing?.Version != null && version <= NuGetVersion.Parse(existing.Version))
                        {
                            log.WriteLine("Ignoring due to existing version " + existing.Version + ": " + pageEntry.Id + " " + pageEntry.Version);
                            continue;
                        }

                        // Ensure the package supports netstandard.
                        var package = await pageEntry.GetPackageAsync();
                        var frameworks = package.Metadata().GetSupportedFrameworksWithRef().ToArray();
                        if (!frameworks.Any(x => new FrameworkName(x.DotNetFrameworkName).IsNetStandard()))
                        {
                            log.WriteLine("Ignoring due to platform: " + pageEntry.Id + " " + pageEntry.Version);
                            continue;
                        }

                        // Add a process request.
                        log.WriteLine("Will process: " + pageEntry.Id + " " + pageEntry.Version);
                        await processPackageQueue.AddAsync(new IndexPackageRequest
                        {
                            PackageId = pageEntry.Id,
                            PackageVersion = pageEntry.Version,
                        });
                        //Console.WriteLine(count.ToString("X") + "/" + set.Count.ToString("X") + ": " + id.Id + " " + id.Version + ", " + package.BestGuessPublicationDate + ", " + package.IsPrerelease + ", " + package.IsListed);

                        // Mark it as awaiting processing.
                        await table.SetVersionAsync(pageEntry.Id, pageEntry.Version, pageEntry.CommitId);
                    }
                }
            }
        }

        private static IEnumerable<CatalogPageEntry> Fixup(IEnumerable<CatalogPageEntry> entries)
        {
            // The json files are often out-of-order and not even unique across (id, version). So this function puts at least this page's entries in an appropriate order.
            // TODO: Require 'commitId' for bookmarks!
            return entries
                .Distinct(EqualityComparerBuilder.For<CatalogPageEntry>().EquateBy(x => x.Id.ToLowerInvariant() + "@" + (NuGetVersion.Parse(x.Version).IsPrerelease ? "1" : "0")))
                .OrderByDescending(x => x.Version).ThenByDescending(x => x.CommitTimestamp);
        }
    }
}
