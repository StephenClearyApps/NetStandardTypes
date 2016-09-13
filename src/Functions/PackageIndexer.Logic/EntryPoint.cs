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
using NuGet.Packaging;
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

            var netstandardFrameworks = new HashSet<FrameworkName>(EqualityComparerBuilder.For<FrameworkName>().EquateBy(x => x.FullName, StringComparer.InvariantCultureIgnoreCase));
            foreach (var target in package.GetSupportedFrameworks().Where(IsNetStandard))
                netstandardFrameworks.Add(target);
            var supportedFrameworks = netstandardFrameworks.OrderBy(x => x.Version).ToArray();

            foreach (var framework in supportedFrameworks)
            {
                var current = new PackageDocument
                {
                    Id = AzureSearchUtilities.EncodeDocumentKey(request.PackageId + "$" + (request.PackageVersion.Contains('-') ? 1 : 0)),
                    PackageId = request.PackageId,
                    PackageVersion = request.PackageVersion,
                    Preview = request.PackageVersion.Contains('-'),
                    Published = package.Published.Value,
                    TotalDownloadCount = package.DownloadCount,
                    NetstandardVersion = framework.Version.Major * 256 + framework.Version.Minor,
                };
                foreach (var path in GetCompatibleAssemblyReferences(packageReader, framework).Where(x => x.EndsWith(".dll")))
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
                        var name = type.Namespace + "." + TypeName(type);
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

        private static bool IsNetStandard(FrameworkName frameworkName)
        {
            var name = Prefix(frameworkName.Identifier);
            return name.Equals(".netstandard", StringComparison.InvariantCultureIgnoreCase);
        }

        private static readonly char[] Numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        private static string Prefix(string framework)
        {
            var prefixOffset = framework.IndexOfAny(Numbers);
            return prefixOffset == -1 ? framework : framework.Substring(0, prefixOffset);
        }

        private static string TypeName(TypeDefinition type)
        {
            var name = type.Name.StripBacktickSuffix();
            var result = name.Name;
            if (name.Value > 0)
                result += "<" + string.Join(", ", type.GenericParameters.Take(name.Value).Select(x => x.Name)) + ">";
            return result;
        }

        private static IEnumerable<string> GetCompatibleAssemblyReferences(PackageArchiveReaderWithRef package, FrameworkName target)
        {
            var framework = NuGetFramework.ParseFrameworkName(target.FullName, DefaultFrameworkNameProvider.Instance);
            var result = NuGetFrameworkUtility.GetNearest(package.GetRefItems(), framework);
            if (result != null)
            {
                var items = result.Items.ToArray();
                if (items.Length != 0)
                    return items;
            }
            result = NuGetFrameworkUtility.GetNearest(package.GetLibItems(), framework);
            return result == null ? Enumerable.Empty<string>() : result.Items;
        }

        private sealed class PackageArchiveReaderWithRef : PackageArchiveReader
        {
            public PackageArchiveReaderWithRef(Stream stream, bool leaveStreamOpen)
                : base(stream, leaveStreamOpen)
            {
            }

            public IEnumerable<FrameworkSpecificGroup> GetRefItems()
            {
                return GetFileGroups(PackagingConstants.Folders.Ref);
            }

            public new IEnumerable<NuGetFramework> GetSupportedFrameworks()
            {
                var frameworks = new HashSet<NuGetFramework>(new NuGetFrameworkFullComparer());
                frameworks.UnionWith(base.GetSupportedFrameworks());
                frameworks.UnionWith(GetRefItems().Select(x => x.TargetFramework));
                return frameworks.OrderBy(x => x, new NuGetFrameworkSorter());
            }
        }

    }
}
