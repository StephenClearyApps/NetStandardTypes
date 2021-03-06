﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Mono.Cecil;
using NetStandardTypes.NuGetHelpers;
using Nito.Comparers;
using NuGet;
using Polly;

namespace NetStandardTypes.PackageIndexer
{
    public static class EntryPoint
    {
        public static async Task Run(IndexPackageRequest request, TextWriter log)
        {
            log.WriteLine("Starting: " + request.PackageId + " " + request.PackageVersion);

            var client = Config.CreateSearchServiceClient().Indexes.GetClient(Config.IndexName);
            var documents = GenerateDocuments(request, log).ToList();
            var batch = IndexBatch.Upload(documents);

            await Policy.Handle<IndexBatchException>()
                .RetryAsync((ex, context) =>
                {
                    batch = ((IndexBatchException) ex).FindFailedActionsToRetry(batch, x => x.Id);
                })
                .ExecuteAsync(() => client.Documents.IndexAsync(batch));
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
                    Prerelease = request.PackageVersion.Contains('-'),
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
                            current.TypesCamelHump.Add(type.TypeName());
                        }
                    }
                }

                yield return current;
            }
        }

        private static FrameworkName[] SupportedFrameworks(PackageArchiveReaderWithRef packageReader)
        {
            var netstandardFrameworks = new HashSet<FrameworkName>(EqualityComparerBuilder.For<FrameworkName>().EquateBy(x => x.FullName, StringComparer.InvariantCultureIgnoreCase));
            foreach (var target in packageReader.GetSupportedFrameworksWithRef()
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
