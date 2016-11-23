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
        private class PackageEntryBase
        {
            public PackageEntryBase(string lowercasePackageId, NuGetVersion packageVersion)
            {
                PackageVersion = packageVersion;
                Key = lowercasePackageId + "@" + (packageVersion.IsPrerelease ? "1" : "0");
            }

            public string Key { get; private set; }
            public NuGetVersion PackageVersion { get; set; }
        }

        private sealed class PackageEntry : PackageEntryBase
        {
            public PackageEntry(string commitId, string lowercasePackageId, NuGetVersion packageVersion)
                : base(lowercasePackageId, packageVersion)
            {
                CommitId = commitId;
                LowercasePackageId = lowercasePackageId;
            }

            public string CommitId { get; private set; }
            public string LowercasePackageId { get; private set; }
        }

        private sealed class Results
        {
            public List<PackageEntry> PackagesToProcess { get; } = new List<PackageEntry>();
            public string Bokmark { get; set; }
        }

        private static async Task<Results> InspectCatalogAsync(PackageTable table, TextWriter log)
        {
            var ignoredDueToPlatform = new List<PackageEntryBase>();
            var result = new Results();
            var index = await ServiceIndex.CreateAsync();
            var catalog = await index.GetCatalogAsync();
            string bookmark = null;
            var bookmarkFound = false;
            foreach (var catalogItem in catalog.Items)
            {
                var page = await catalogItem.GetCatalogPageAsync();
                foreach (var pageItem in page.Items)
                {
                    if (pageItem.CommitId == bookmark)
                        bookmarkFound = true;

                    // Parse the version.
                    NuGetVersion version;
                    if (!NuGetVersion.TryParse(pageItem.Version, out version))
                    {
                        log.WriteLine("Unable to parse version: " + pageItem.Id + " " + pageItem.Version);
                        continue;
                    }

                    // Generate the key (id + prerelease flag).
                    var lowercasePackageId = pageItem.Id.ToLowerInvariant();
                    var key = lowercasePackageId + (version.IsPrerelease ? "1" : "0");

                    // Check to see if we have a newer in-memory entry already processed for this key.
                    var resultExisting = result.PackagesToProcess.FirstOrDefault(x => x.Key == key);
                    if (resultExisting != null && version <= resultExisting.PackageVersion)
                    {
                        log.WriteLine("Already going to process " + resultExisting.PackageVersion + ": " + pageItem.Id + " " + pageItem.Version);
                        continue;
                    }

                    // Check to see if we have a newer in-memory ignored entry already processed for this key.
                    var ignoredExisting = ignoredDueToPlatform.FirstOrDefault(x => x.Key == key);
                    if (ignoredExisting != null && version <= ignoredExisting.PackageVersion)
                    {
                        log.WriteLine("Already ignored due to platform: " + pageItem.Id + " " + pageItem.Version);
                        continue;
                    }

                    // Look up the existing table entry, if any.
                    var existing = await table.TryGetVersionCommitAsync(pageItem.Id, pageItem.Version);
                        
                    // If the existing table entry is this same exact commit, then we're done.
                    if (existing != null && string.Equals(existing.CommitId, pageItem.CommitId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        log.WriteLine("Already processed: " + pageItem.Id + " " + pageItem.Version + " at " + pageItem.CommitId);
                        continue;
                    }

                    // If the existing table entry is newer than this one, then skip this one.
                    //  (This can only happen if NuGet packages are published out of order. Which *does* seem to happen!)
                    if (existing?.Version != null && version <= NuGetVersion.Parse(existing.Version))
                    {
                        log.WriteLine("Ignoring due to existing version " + existing.Version + ": " + pageItem.Id + " " + pageItem.Version);
                        continue;
                    }

                    // Ensure the package supports netstandard.
                    var package = await pageItem.GetPackageAsync();
                    var frameworks = package.Metadata().GetSupportedFrameworksWithRef().ToArray();
                    if (!frameworks.Any(x => new FrameworkName(x.DotNetFrameworkName).IsNetStandard()))
                    {
                        log.WriteLine("Ignoring due to platform: " + pageItem.Id + " " + pageItem.Version);
                        if (ignoredExisting != null)
                            ignoredExisting.PackageVersion = version;
                        else
                            ignoredDueToPlatform.Add(new PackageEntryBase(lowercasePackageId, version));
                        continue;
                    }

                    // Add a process request.
                    log.WriteLine("Will process: " + pageItem.Id + " " + pageItem.Version);
                    result.PackagesToProcess.Add(new PackageEntry(pageItem.CommitId, lowercasePackageId, version));
                }

                if (bookmarkFound)
                    return result;
            }
            return result;
        }

        public static async Task Run(TimerInfo myTimer, IAsyncCollector<IndexPackageRequest> processPackageQueue, TextWriter log)
        {
            var table = new PackageTable();
            var mutex = new AzureLock(Config.CreateCloudBlobClient().GetContainerReference("locks").GetBlockBlobReference("nugetwalker"));
            using (await mutex.LockAsync())
            {
                // Parse the entire catalog, until the bookmark.
                var actions = await InspectCatalogAsync(table, log);
                log.WriteLine("Done examining catalog; found: " + actions.PackagesToProcess.Count);

                // In catalog order (oldest to newest), place the message in the queue and then update the table.
                for (int i = actions.PackagesToProcess.Count - 1; i >= 0; --i)
                {
                    var action = actions.PackagesToProcess[i];
                    await processPackageQueue.AddAsync(new IndexPackageRequest
                    {
                        PackageId = action.LowercasePackageId,
                        PackageVersion = action.PackageVersion.ToString(),
                    });
                    await processPackageQueue.FlushAsync();

                    // Mark it as awaiting processing.
                    await table.SetVersionAsync(action.LowercasePackageId, action.PackageVersion, action.CommitId);
                }

                // TODO: Update bookmark
            }
        }
    }
}
