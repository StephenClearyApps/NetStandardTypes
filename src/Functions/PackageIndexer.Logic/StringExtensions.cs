using System.Globalization;

namespace NetStandardTypes.PackageIndexer
{
    public static class StringExtensions
    {
        /// <summary>
        /// Removes the backtick suffix (if any) of the string. Works for double-backtick suffixed strings as well.
        /// </summary>
        public static BacktickStrippedString StripBacktickSuffix(this string s)
        {
            var backtickIndex = s.IndexOf('`');
            if (backtickIndex == -1)
                return new BacktickStrippedString { Name = s };
            return new BacktickStrippedString { Name = s.Substring(0, backtickIndex), Value = int.Parse(s.Substring(s.LastIndexOf('`') + 1), CultureInfo.InvariantCulture) };
        }

        /// <summary>
        /// A string that has had its backtick suffix stripped.
        /// </summary>
        public struct BacktickStrippedString
        {
            /// <summary>
            /// Gets the name with the backtick suffix stripped.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets the integral value that was after the backtick(s). If there was no backtick, then this is <c>0</c>.
            /// </summary>
            public int Value { get; set; }
        }
    }
}
