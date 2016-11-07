using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateInfrastructure
{
    public static class Config
    {
        public static string AzureSearchKey { get; } = Environment.GetEnvironmentVariable("NETSTANDARDTYPES_SEARCHKEY");
        public static string AzureStorageKey { get; } = Environment.GetEnvironmentVariable("NETSTANDARDTYPES_STORAGEKEY");
    }
}
