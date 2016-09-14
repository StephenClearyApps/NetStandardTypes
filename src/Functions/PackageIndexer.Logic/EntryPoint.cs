using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Nito.Comparers;
using NuGet;
using NuGet.Frameworks;
using Polly;

namespace PackageIndexer.Logic
{
    public static class EntryPoint
    {

        public static void Run(IndexPackageRequest request, TextWriter log)
        {
            log.WriteLine("Starting: " + request.PackageId + " " + request.PackageVersion);

            var client = new SearchServiceClient("netstandardtypes", new SearchCredentials(Config.AzureSearchKey));

            // TODO: polly
            try
            {
                var documents = GenerateDocuments(request, log).ToArray();
                var batch = IndexBatch.Upload(documents);
                client.Indexes.GetClient("types").Documents.Index(batch);
            }
            catch (IndexBatchException e)
            {
                // Sometimes when your Search service is under load, indexing will fail for some of the documents in
                // the batch. Depending on your application, you can take compensating actions like delaying and
                // retrying.

                // e.IndexingResults.Where(r => !r.Succeeded)
                throw;
            }
        }

        private static IEnumerable<PackageDocument> GenerateDocuments(IndexPackageRequest request, TextWriter log)
        {
            var namespaces = new HashSet<string>();
            var types = new HashSet<string>();

            var nuget = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");
            var packageDownloadPolicy = Policy.HandleResult((IPackage) null).WaitAndRetry(10, _ => TimeSpan.FromMinutes(1));
            var package = packageDownloadPolicy.Execute(() => nuget.FindPackage(request.PackageId, SemanticVersion.Parse(request.PackageVersion), true, false));
            if (package == null)
            {
                log.WriteLine("Package download failed for " + request);
                throw new InvalidOperationException("Package download failed for " + request);
            }
            var packageStream = package.GetStream();
            var packageReader = new PackageArchiveReaderWithRef(packageStream, leaveStreamOpen: true);

            foreach (var framework in SupportedFrameworks(packageReader))
            {
                var netstandardVersion = framework.Version.Major * 256 + framework.Version.Minor;
                var current = new PackageDocument
                {
                    Id = AzureSearchUtilities.EncodeDocumentKey(request.PackageId + "$" + (request.PackageVersion.Contains('-') ? 1 : 0) + "$" + netstandardVersion.ToString("X4")),
                    PackageId = request.PackageId,
                    PackageVersion = request.PackageVersion,
                    Preview = request.PackageVersion.Contains('-'),
                    Published = package.Published.Value,
                    TotalDownloadCount = package.DownloadCount,
                    NetstandardVersion = netstandardVersion,
                };
                foreach (var path in packageReader.GetCompatibleAssemblyReferences(framework).Where(x => x.EndsWith(".dll")))
                {
                    var dllStream = new MemoryStream();
                    packageReader.GetStream(path).CopyTo(dllStream);
                    dllStream.Position = 0;

                    var readerParameters = new ReaderParameters
                    {
                        AssemblyResolver = new NullAssemblyResolver(log),
                    };
                    var assembly = AssemblyDefinition.ReadAssembly(dllStream, readerParameters);

                    foreach (var type in assembly.Modules.SelectMany(x => x.Types).Where(x => x.IsPublic))
                    {
                        if (!namespaces.Contains(type.Namespace))
                        {
                            namespaces.Add(type.Namespace);
                            current.Namespaces.Add(type.Namespace);
                        }
                        var name = type.Namespace + "." + type.TypeName();
                        if (!types.Contains(name))
                        {
                            types.Add(name);
                            current.Types.Add(name);
                        }
                    }
                }

                yield return current;
            }
        }

        private static FrameworkName[] SupportedFrameworks(PackageArchiveReaderWithRef packageReader)
        {
            var netstandardFrameworks = new HashSet<FrameworkName>(EqualityComparerBuilder.For<FrameworkName>().EquateBy(x => x.FullName, StringComparer.InvariantCultureIgnoreCase));
            foreach (var target in packageReader.GetSupportedFrameworks()
                .Select(x => new FrameworkName(x.DotNetFrameworkName))
                .Where(x => x.IsNetStandard()))
            {
                netstandardFrameworks.Add(target);
            }
            var supportedFrameworks = netstandardFrameworks.OrderBy(x => x.Version).ToArray();
            return supportedFrameworks;
        }
    }
}
