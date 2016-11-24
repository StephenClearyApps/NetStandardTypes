using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

            public string Key { get; }
            public NuGetVersion PackageVersion { get; set; }
        }

        private sealed class PackageEntry : PackageEntryBase
        {
            public PackageEntry(int page, string commitId, string lowercasePackageId, NuGetVersion packageVersion)
                : base(lowercasePackageId, packageVersion)
            {
                Page = page;
                CommitId = commitId;
                LowercasePackageId = lowercasePackageId;
            }

            public int Page { get; }
            public string CommitId { get; }
            public string LowercasePackageId { get; }
        }

        private sealed class Results
        {
            public List<PackageEntry> PackagesToProcess { get; } = new List<PackageEntry>();
        }

        private static async Task<Results> InspectCatalogAsync(PackageTable table, TextWriter log)
        {
            var ignoredDueToPlatform = new List<PackageEntryBase>();
            var result = new Results();
            var index = await ServiceIndex.CreateAsync();
            var catalog = await index.GetCatalogAsync();
            var bookmarkFound = false;

            // Catalog page 1532 contains the first library that supported netstandard: csharp2colorized 1.0.0-beta1
            for (int i = catalog.Items.Count - 1; i >= 1532; --i)
            {
                var page = await catalog.Items[i].GetCatalogPageAsync();
                foreach (var pageItem in page.Items)
                {
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
                        bookmarkFound = true;
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
                    result.PackagesToProcess.Add(new PackageEntry(i, pageItem.CommitId, lowercasePackageId, version));
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

                // In catalog order (oldest page to newest page), place all the messages in the queue and then update the table entries.
                foreach (var group in actions.PackagesToProcess.GroupBy(x => x.Page).OrderBy(x => x.Key))
                {
                    foreach (var action in group)
                    {
                        await processPackageQueue.AddAsync(new IndexPackageRequest
                        {
                            PackageId = action.LowercasePackageId,
                            PackageVersion = action.PackageVersion.ToString(),
                        });
                    }
                    await processPackageQueue.FlushAsync();

                    foreach (var action in group)
                        await table.SetVersionAsync(action.LowercasePackageId, action.PackageVersion, action.CommitId);
                }
            }
        }
    }
}
