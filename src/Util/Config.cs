using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Azure.Search;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Util
{
    public static class Config
    {
        public static string AzureSearchKey { get; } = CloudConfigurationManager.GetSetting("NETSTANDARDTYPES_SEARCHKEY") ??
            Environment.GetEnvironmentVariable("NETSTANDARDTYPES_SEARCHKEY");
        private static string AzureStorageKey { get; } = CloudConfigurationManager.GetSetting("NETSTANDARDTYPES_STORAGEKEY") ??
            Environment.GetEnvironmentVariable("NETSTANDARDTYPES_STORAGEKEY");

        private static Uri SearchUri { get; } = new Uri("https://netstandardtypes.search.windows.net");
        private static Uri QueueUri { get; } = new Uri("https://netstandardtypes.queue.core.windows.net/");
        private static HttpClientHandler HttpClientHandler { get; } = new HttpClientHandler();
        private static SearchCredentials SearchCredentials { get; } = new SearchCredentials(AzureSearchKey);
        private static StorageCredentials StorageCredentials { get; } = new StorageCredentials("netstandardtypes", AzureStorageKey);

        public static SearchServiceClient CreateSearchServiceClient() => new SearchServiceClient(SearchUri, SearchCredentials, HttpClientHandler);

        public static CloudQueueClient CreateCloudQueueClient() => new CloudQueueClient(QueueUri, StorageCredentials);
    }
}
