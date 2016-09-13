using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageIndexer.Logic
{
    public sealed class IndexPackageRequest
    {
        public string PackageId { get; set; }

        public string PackageVersion { get; set; }

        public override string ToString()
        {
            return PackageId + " " + PackageVersion;
        }
    }
}
