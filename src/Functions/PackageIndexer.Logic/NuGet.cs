using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageIndexer.Logic
{
    public sealed class NuGet
    {
        private readonly IPackageRepository _repository = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");

        /// <summary>
        /// Downloads a specific package from Nuget.
        /// </summary>
        /// <param name="idver">The identity of the package.</param>
        public NugetPackage DownloadPackageAsync(NugetPackageIdVersion idver)
        {
            var package = _repository.FindPackage(idver.PackageId, idver.Version.ToSemanticVersion(), true, true);
            if (package == null)
                return Task.FromException<NugetFullPackage>(new BusinessException(HttpStatusCode.NotFound, "Could not find package " + idver, "This error can happen if NuGet is currently indexing this package.\nIf this is a newly released version, try again in 5 minutes or so."));
            return Task.FromResult(new NugetFullPackage(new NugetPackage(package.GetStream()), new NugetPackageExternalMetadata(package.Published.Value)));
        }
    }
}
