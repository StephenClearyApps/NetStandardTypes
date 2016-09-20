using System;
using System.Runtime.Versioning;

namespace NuGetHelpers
{
    public static class FrameworkNameExtensions
    {
        public static bool IsNetStandard(this FrameworkName frameworkName)
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

    }
}
