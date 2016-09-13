using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;

namespace PackageIndexer.Logic
{
    public static class Config
    {
        public static string AzureSearchKey { get; } = CloudConfigurationManager.GetSetting("NETSTANDARDTYPES_SEARCHKEY") ??
            Environment.GetEnvironmentVariable("NETSTANDARDTYPES_SEARCHKEY");
    }
}
