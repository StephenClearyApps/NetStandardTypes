using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

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
                    new Field("namespaces", DataType.Collection(DataType.String)) { IsRetrievable = false, IsFilterable = true, IsSortable = false, IsFacetable = true, IsSearchable = true },
                    new Field("types", DataType.Collection(DataType.String)) { IsRetrievable = false, IsFilterable = false, IsSortable = false, IsFacetable = false, IsSearchable = true },
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
                    MaxAgeInSeconds = (long)TimeSpan.FromHours(2).TotalSeconds,
                },
            };
        }
    }
}
