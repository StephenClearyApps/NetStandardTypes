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

                EntryPoint.Run(new IndexPackageRequest()
                {
                    PackageId = "Nito.AsyncEx.Coordination",
                    PackageVersion = "1.0.2",
                }, Console.Out);

                EntryPoint.Run(new IndexPackageRequest()
                {
                    PackageId = "System.Threading.Tasks",
                    PackageVersion = "4.0.11",
                }, Console.Out);

                Console.WriteLine("Done.");
                // &highlight=namespaces,types,typesCamelHump&highlightPreTag=$&highlightPostTag=$&search=al
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
                    new Field("namespaces", DataType.Collection(DataType.String)) { IsRetrievable = false, IsFilterable = true, IsSortable = false, IsFacetable = true, IsSearchable = true, IndexAnalyzer = AnalyzerName.Create("name_index"), SearchAnalyzer = AnalyzerName.Create("name_search") },
                    new Field("types", DataType.Collection(DataType.String)) { IsRetrievable = false, IsFilterable = false, IsSortable = false, IsFacetable = false, IsSearchable = true, IndexAnalyzer = AnalyzerName.Create("name_index"), SearchAnalyzer = AnalyzerName.Create("name_search") },
                    new Field("typesCamelHump", DataType.Collection(DataType.String)) { IsRetrievable = false, IsFilterable = false, IsSortable = false, IsFacetable = false, IsSearchable = true, IndexAnalyzer = AnalyzerName.Create("camel_hump_index"), SearchAnalyzer = AnalyzerName.Create("camel_hump_search") },
                    new Field("packageId", DataType.String) { IsRetrievable = true, IsFilterable = false, IsSortable = false, IsFacetable = false, IsSearchable = false },
                    new Field("packageVersion", DataType.String) { IsRetrievable = true, IsFilterable = false, IsSortable = false, IsFacetable = false, IsSearchable = false },
                    new Field("prerelease", DataType.Boolean) { IsRetrievable = false, IsFilterable = true, IsSortable = false, IsFacetable = true, IsSearchable = false },
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
                    new PatternReplaceCharFilter
                    {
                        Name = "remove_generics",
                        Pattern = "<[^>]*>",
                        Replacement = ".",
                    },
                    new PatternReplaceCharFilter
                    {
                        Name = "remove_non_uppercase",
                        Pattern = "[^A-Z]+",
                        Replacement = ".",
                    },
                    new MappingCharFilter
                    {
                        Name = "period_to_space",
                        Mappings = new[] { @".=>\u0020" },
                    },
                    new MappingCharFilter
                    {
                        Name = "period_to_empty_string",
                        Mappings = new[] { @".=>" },
                    },
                },
                TokenFilters = new TokenFilter[]
                {
                    new EdgeNGramTokenFilter
                    {
                        Name = "my_ngram",
                        MinGram = 2,
                        MaxGram = 16,
                        Side = EdgeNGramTokenFilterSide.Front,
                    },
                },
                Analyzers = new Analyzer[]
                {
                    new CustomAnalyzer
                    {
                        Name = "name_search",
                        CharFilters = new []
                        {
                            CharFilterName.Create("remove_generics"),
                            CharFilterName.Create("period_to_space"),
                        },
                        Tokenizer = TokenizerName.Whitespace,
                        TokenFilters = new []
                        {
                            TokenFilterName.Lowercase, 
                        },
                    },
                    new CustomAnalyzer
                    {
                        Name = "name_index",
                        CharFilters = new []
                        {
                            CharFilterName.Create("remove_generics"),
                            CharFilterName.Create("period_to_space"),
                        },
                        Tokenizer = TokenizerName.Whitespace,
                        TokenFilters = new []
                        {
                            TokenFilterName.Lowercase,
                            TokenFilterName.Create("my_ngram"),
                        },
                    },
                    new CustomAnalyzer
                    {
                        Name = "camel_hump_search",
                        Tokenizer = TokenizerName.Whitespace,
                        TokenFilters = new []
                        {
                            TokenFilterName.Lowercase,
                        },
                    },
                    new CustomAnalyzer
                    {
                        Name = "camel_hump_index",
                        CharFilters = new []
                        {
                            CharFilterName.Create("remove_generics"),
                            CharFilterName.Create("remove_non_uppercase"),
                            CharFilterName.Create("period_to_empty_string"),
                        },
                        Tokenizer = TokenizerName.Keyword,
                        TokenFilters = new []
                        {
                            TokenFilterName.Lowercase,
                            TokenFilterName.Create("my_ngram"),
                        },
                    },
                },
            };
        }
    }
}
