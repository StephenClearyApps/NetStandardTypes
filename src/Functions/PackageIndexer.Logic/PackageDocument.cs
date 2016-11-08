using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Search.Models;

namespace NetStandardTypes.PackageIndexer
{
    [SerializePropertyNamesAsCamelCase]
    public sealed class PackageDocument
    {
        public string Id { get; set; }

        public List<string> Namespaces { get; set; } = new List<string>();

        public List<string> Types { get; set; } = new List<string>();

        public List<string> TypesCamelHump { get; set; } = new List<string>();

        public string PackageId { get; set; }

        public string PackageVersion { get; set; }

        public bool Prerelease { get; set; }

        public int NetstandardVersion { get; set; }

        public DateTimeOffset Published { get; set; }

        public int TotalDownloadCount { get; set; }

        public void AddNamespace(string @namespace)
        {
            Namespaces.Add(@namespace);
        }

        private static IEnumerable<string> SplitByCamelCase(string text)
        {
            var sb = new StringBuilder();
            for (int i = 0; i != text.Length; ++i)
            {
                if (char.IsUpper(text[i]))
                {
                    var result = sb.ToString();
                    if (result != "")
                        yield return result;
                    sb = new StringBuilder();
                }
                sb.Append(text[i]);
            }
            var finalResult = sb.ToString();
            if (finalResult != "")
                yield return finalResult;
        }

        private static string UppercaseAbbreviation(string text)
        {
            return new string(text.Where(char.IsUpper).ToArray());
        }
    }
}