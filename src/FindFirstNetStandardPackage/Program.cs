using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using NetStandardTypes.NuGetHelpers;
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

        // Page 577

        // Next page: https://api.nuget.org/v3/catalog0/page1532.json
        // Found: csharp2colorized 1.0.2-beta3

        static async Task MainAsync()
        {
            var index = await ServiceIndex.CreateAsync();
            var catalog = await index.GetCatalogAsync(reversed: false);
            using (var pageEnumerator = catalog.Pages().GetEnumerator())
            {
                while (await pageEnumerator.MoveNext())
                {
                    var entries = pageEnumerator.Current.Entries().ToList();
                    Console.WriteLine(DateTimeOffset.Now + ": Processing " + entries.Count);

                    foreach (var pageEntry in pageEnumerator.Current.Entries())
                    {
                        // Ensure the package supports netstandard.
                        var package = await pageEntry.GetPackageAsync();
                        var frameworks = package.Metadata().GetSupportedFrameworksWithRef().ToArray();
                        if (frameworks.Any(x => new FrameworkName(x.DotNetFrameworkName).IsNetStandard()))
                        {
                            Console.WriteLine("Found: " + pageEntry.Id + " " + pageEntry.Version);
                            return;
                        }
                    }
                }

            }
        }
    }
}
