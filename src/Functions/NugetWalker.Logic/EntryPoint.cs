using System;
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

namespace NetStandardTypes.NugetWalker
{
    public static class EntryPoint
    {
        private sealed class PackageEntry
        {
            public string CommitId { get; set; }
            public string LowercasePackageId { get; set; }
            public NuGetVersion PackageVersion { get; set; }
        }

        private static async Task<List<PackageEntry>> InspectCatalogAsync(PackageTable table, TextWriter log)
        {
            var result = new List<PackageEntry>();
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
                            log.WriteLine("Unable to parse version: " + pageEntry.Id + " " + pageEntry.Version);
                            continue;
                        }

                        // Look up the existing table entry, if any.
                        var existing = await table.TryGetVersionCommitAsync(pageEntry.Id, pageEntry.Version);
                        
                        // TODO: Change to a global bookmark and pass it to the catalog enumerator. This allows json files to be prepended or appended to.
                        // If the existing table entry is this same exact commit, then we're done.
                        if (existing != null && string.Equals(existing.CommitId, pageEntry.CommitId, StringComparison.InvariantCultureIgnoreCase))
                        {
                            log.WriteLine("Reached bookmark: " + pageEntry.Id + " " + pageEntry.Version + " at " + pageEntry.CommitId);
                            return result;
                        }

                        // If the existing table entry is newer than this one, then skip this one.
                        //  (This can only happen if NuGet packages are published out of order. Which *does* seem to happen!)
                        if (existing?.Version != null && version <= NuGetVersion.Parse(existing.Version))
                        {
                            log.WriteLine("Ignoring due to existing version " + existing.Version + ": " + pageEntry.Id + " " + pageEntry.Version);
                            continue;
                        }

                        // Check to see if we have a newer in-memory entry already processed for this id.
                        var resultExisting = result.FirstOrDefault(x => string.Equals(x.LowercasePackageId, pageEntry.Id, StringComparison.InvariantCultureIgnoreCase));
                        if (resultExisting != null && version <= resultExisting.PackageVersion)
                        {
                            log.WriteLine("Ignoring due to existing in-memory version " + resultExisting.PackageVersion + ": " + pageEntry.Id + " " + pageEntry.Version);
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
                        result.Add(new PackageEntry
                        {
                            CommitId = pageEntry.CommitId,
                            LowercasePackageId = pageEntry.Id.ToLowerInvariant(),
                            PackageVersion = version,
                        });
                    }
                }
            }
            return result;
        }

        public static async Task Run(TimerInfo myTimer, IAsyncCollector<IndexPackageRequest> processPackageQueue, TextWriter log)
        {
            var table = new PackageTable();
            var client = Config.CreateCloudBlobClient();
            var container = client.GetContainerReference("locks");
            await container.CreateIfNotExistsAsync();
            var mutex = new AzureLock(container.GetBlockBlobReference("nugetwalker"));
            using (await mutex.LockAsync())
            {
                // Parse the entire catalog, until the bookmark.
                var actions = await InspectCatalogAsync(table, log);
                log.WriteLine("Done examining catalog; found: " + actions.Count);

                // In catalog order (oldest to newest), place the message in the queue and then update the table.
                for (int i = actions.Count - 1; i >= 0; --i)
                {
                    var action = actions[i];
                    await processPackageQueue.AddAsync(new IndexPackageRequest
                    {
                        PackageId = action.LowercasePackageId,
                        PackageVersion = action.PackageVersion.ToString(),
                    });
                    await processPackageQueue.FlushAsync();

                    // Mark it as awaiting processing.
                    await table.SetVersionAsync(action.LowercasePackageId, action.PackageVersion, action.CommitId);
                }
            }
        }

        private static IEnumerable<CatalogPageEntry> Fixup(IEnumerable<CatalogPageEntry> entries)
        {
            // The json files are often out-of-order and not even unique across (id, version). So this function puts at least this page's entries in an appropriate order.
            return entries
                .Distinct(EqualityComparerBuilder.For<CatalogPageEntry>().EquateBy(x => x.Id.ToLowerInvariant() + "@" + (NuGetVersion.Parse(x.Version).IsPrerelease ? "1" : "0")))
                .OrderByDescending(x => x.Version).ThenByDescending(x => x.CommitTimestamp);
        }
    }
}
