using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using NetStandardTypes.NuGetHelpers;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuGetCatalog;

namespace FindFirstNetStandardPackage
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MainAsync().GetAwaiter().GetResult();
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        // https://api.nuget.org/v3/catalog0/page1532.json
        // Found: csharp2colorized 1.0.2-beta3
        // csharp2colorized 1.0.0-beta1

        static async Task MainAsync()
        {
            var index = await ServiceIndex.CreateAsync();
            var catalog = await index.GetCatalogAsync();
            foreach (var catalogItem in catalog.Items)
            {
                var page = await catalogItem.GetCatalogPageAsync();
                Console.WriteLine(DateTimeOffset.Now + ": Processing " + page.Items.Count);
                bool found = false;
                foreach (var pageItem in page.Items)
                {
                    // Ensure the package supports netstandard.
                    var package = await pageItem.GetPackageAsync();
                    var frameworks = TryGetSupportedFrameworks(package);
                    if (frameworks.Any(x => new FrameworkName(x.DotNetFrameworkName).IsNetStandard()))
                    {
                        Console.WriteLine("Found: " + pageItem.Id + " " + pageItem.Version);
                        found = true;
                    }
                }
                if (found)
                    return;
            }
        }

        static NuGetFramework[] TryGetSupportedFrameworks(CatalogPackage package)
        {
            try
            {
                return package.Metadata().GetSupportedFrameworksWithRef().ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing " + package.Identity + ": " + ex.Message);
                if (!KnownBadPackages.Contains(package.Identity))
                    throw;
                return new NuGetFramework[0];
            }
        }

        private static readonly HashSet<PackageIdentity> KnownBadPackages = new HashSet<PackageIdentity>()
        {
            new PackageIdentity("dingu.generic.repo.ef7", NuGetVersion.Parse("1.0.0-beta2")),
            new PackageIdentity("dingu.generic.repo.ef7", NuGetVersion.Parse("1.0.0")),
        };
    }
}
