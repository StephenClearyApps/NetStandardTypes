using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGetHelpers;

namespace NuGetCatalog
{
    public sealed class CatalogPackage
    {
        private readonly JObject _content;

        internal CatalogPackage(JObject content)
        {
            _content = content;
        }

        public CatalogPackageReaderWithRef Metadata()
        {
            return new CatalogPackageReaderWithRef(_content);
        }
    }
}