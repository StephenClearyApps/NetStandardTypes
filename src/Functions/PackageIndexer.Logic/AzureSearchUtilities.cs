using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageIndexer.Logic
{
    public static class AzureSearchUtilities
    {
        /// <summary>
        /// Encodes a string so that it only contains valid characters usable in a document key (letters, digits, underscore(_), dash(-), or equal sign(=)).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EncodeDocumentKey(string value)
        {
            var sb = new StringBuilder();
            foreach (var ch in value)
            {
                if (ch >= 'a' && ch <= 'z')
                    sb.Append(ch);
                else if (ch >= 'A' && ch <= 'Z')
                    sb.Append(ch);
                else if (ch >= '0' && ch <= '9')
                    sb.Append(ch);
                else if (ch == '_' || ch == '-')
                    sb.Append(ch);
                else
                    sb.Append("=" + ((int) ch).ToString("X4", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        public static string DecodeDocumentKey(string documentKey)
        {
            var sb = new StringBuilder();
            for (int i = 0; i != documentKey.Length; ++i)
            {
                var ch = documentKey[i];
                if (ch != '=')
                    sb.Append(ch);
                else
                {
                    ++i; // Consume '='
                    var chValue = int.Parse(documentKey.Substring(i, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                    sb.Append((char) chValue);
                    i += 4 - 1;
                }
            }
            return sb.ToString();
        }
    }
}
