using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using PackageIndexer.Logic;

namespace CreateInfrastructure
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var serviceClient = new SearchServiceClient("netstandardtypes", new SearchCredentials(Config.AzureSearchKey));

                Console.WriteLine("Deleting index...\n");
                if (serviceClient.Indexes.Exists("types"))
                {
                    serviceClient.Indexes.Delete("types");
                }

                Console.WriteLine("Creating index...\n");
                serviceClient.Indexes.Create(IndexDefinition());

                EntryPoint.Run(new IndexPackageRequest()
                {
                    PackageId = "Nito.Collections.Deque",
                    PackageVersion = "1.0.0",
                }, Console.Out);

                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        private static Index IndexDefinition()
        {
            return new Index()
            {
                Name = "types",
                Fields = new[]
                {
                    new Field("id", DataType.String) { IsKey = true, IsRetrievable = true, IsFilterable = false, IsSortable = false, IsFacetable = false, IsSearchable = false },
                    new Field("namespaces", DataType.Collection(DataType.String)) { IsRetrievable = true, IsFilterable = true, IsSortable = false, IsFacetable = true, IsSearchable = true, Analyzer = AnalyzerName.Create("namespaces") },
                    new Field("types", DataType.Collection(DataType.String)) { IsRetrievable = true, IsFilterable = false, IsSortable = false, IsFacetable = false, IsSearchable = true, Analyzer = AnalyzerName.Create("namespaces") },
                    new Field("packageId", DataType.String) { IsRetrievable = true, IsFilterable = false, IsSortable = false, IsFacetable = false, IsSearchable = false },
                    new Field("packageVersion", DataType.String) { IsRetrievable = true, IsFilterable = false, IsSortable = false, IsFacetable = false, IsSearchable = false },
                    new Field("preview", DataType.Boolean) { IsRetrievable = false, IsFilterable = true, IsSortable = false, IsFacetable = true, IsSearchable = false },
                    new Field("netstandardVersion", DataType.Int32) { IsRetrievable = true, IsFilterable = true, IsSortable = false, IsFacetable = true, IsSearchable = false },
                    new Field("published", DataType.DateTimeOffset) { IsRetrievable = true, IsFilterable = true, IsSortable = true, IsFacetable = false, IsSearchable = false },
                    new Field("totalDownloadCount", DataType.Int32) { IsRetrievable = true, IsFilterable = true, IsSortable = true, IsFacetable = false, IsSearchable = false },
                },
                CorsOptions = new CorsOptions()
                {
                    AllowedOrigins = new[] { "*" },
                    MaxAgeInSeconds = (long) TimeSpan.FromHours(2).TotalSeconds,
                },
                CharFilters = new CharFilter[]
                {
                    new MappingCharFilter
                    {
                        Name = "period_to_space",
                        Mappings = new[] { @".=>\u0020" },
                    },
                    new PatternReplaceCharFilter
                    {
                        Name = "remove_generics",
                        Pattern = "<[^>]*>",
                        Replacement = @"",
                    },
                },
                Analyzers = new Analyzer[]
                {
                    new CustomAnalyzer
                    {
                        Name = "namespaces",
                        CharFilters = new []
                        {
                            CharFilterName.Create("period_to_space"),
                        },
                        Tokenizer = TokenizerName.Whitespace,
                        TokenFilters = new []
                        {
                            TokenFilterName.Lowercase, 
                        },
                    },
                },
            };
        }
    }
}
