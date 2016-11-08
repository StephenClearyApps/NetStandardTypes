using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NetStandardTypes.NuGetHelpers;
using Newtonsoft.Json.Linq;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
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

        public bool IsPrerelease => (bool) _content["isPrerelease"];
        public bool IsListed => (bool) _content["listed"];
        public DateTimeOffset? Published => _content.GetDateTimeOffset("published");
        public DateTimeOffset? Created => _content.GetDateTimeOffset("created");
        public DateTimeOffset? LastEdited => _content.GetDateTimeOffset("lastEdited");

        // Created is highest priority, as per comment here: https://github.com/NuGet/NuGet.Services.Metadata/blob/bff7b323a6e647e94e380770657d3ce71cc88e94/src/Ng/Feed2Catalog.cs#L19
        public DateTimeOffset? BestGuessPublicationDate => Valid(Created) ? Created : Valid(Published) ? Published : Valid(LastEdited) ? LastEdited : null;
        public PackageIdentity Identity => new PackageIdentity((string) _content["id"], NuGetVersion.Parse((string) _content["version"]));

        private static readonly DateTimeOffset MinDate = new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private static bool Valid(DateTimeOffset? value)
        {
            return value != null && value.Value > MinDate;
        }
    }
}