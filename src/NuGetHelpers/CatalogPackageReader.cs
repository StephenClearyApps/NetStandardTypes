using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace NetStandardTypes.NuGetHelpers
{
    // Copied from https://github.com/NuGet/NuGet.Services.Metadata/blob/d1d92417238924dc6b6d48d29d4dac4d8a48a9b9/src/NuGet.Indexing/Extraction/CatalogPackageReader.cs
    public class CatalogPackageReader
        : PackageReaderBase, IDisposable
    {
        private readonly JObject _catalogItem;
        private readonly CatalogNuspecReader _catalogNuspecReader;

        public CatalogPackageReader(JObject catalogItem) : base(DefaultFrameworkNameProvider.Instance, DefaultCompatibilityProvider.Instance)
        {
            _catalogItem = catalogItem;
            _catalogNuspecReader = new CatalogNuspecReader(_catalogItem);
        }

        public override Stream GetStream(string path)
        {
            throw new NotSupportedException();
        }

        public override Stream GetNuspec()
        {
            _catalogNuspecReader.NuspecStream.Position = 0;
            return _catalogNuspecReader.NuspecStream;
        }

        public override IEnumerable<string> GetFiles()
        {
            var array = _catalogItem.GetJArray("packageEntries");
            if (array == null)
            {
                yield break;
            }

            foreach (var entry in array)
            {
                yield return (string)entry["fullName"];
            }
        }

        public override IEnumerable<string> GetFiles(string folder)
        {
            return GetFiles().Where(f => f.StartsWith(folder + "/", StringComparison.OrdinalIgnoreCase));
        }

        public override IEnumerable<string> CopyFiles(string destination, IEnumerable<string> packageFiles, ExtractPackageFileDelegate extractFile, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _catalogNuspecReader.Dispose();
            }
        }
    }
}
