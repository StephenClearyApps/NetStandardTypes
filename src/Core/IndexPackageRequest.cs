namespace Util
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
