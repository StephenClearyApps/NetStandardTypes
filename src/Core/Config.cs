using System;
using System.Net.Http;
using Microsoft.Azure;
using Microsoft.Azure.Search;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace NetStandardTypes
{
    public static class Config
    {
        public static string RefreshCatalogQueueName { get; } = "refresh-catalog";
        public static string ProcessPackageQueueName { get; } = "process-package";

        private static string AzureSearchKey { get; } = GetSetting("NETSTANDARDTYPES_SEARCHKEY");
        private static string AzureStorageConnectionString { get; } = GetSetting("NETSTANDARDTYPES_STORAGECONNECTIONSTRING");

        private static Uri SearchUri { get; } = new Uri("https://netstandardtypes.search.windows.net");
        private static Uri QueueUri { get; } = new Uri("https://netstandardtypes.queue.core.windows.net/");
        private static HttpClientHandler HttpClientHandler { get; } = new HttpClientHandler();
        private static SearchCredentials SearchCredentials { get; } = new SearchCredentials(AzureSearchKey);
        private static CloudStorageAccount CloudStorageAccount { get; } = CloudStorageAccount.Parse(AzureStorageConnectionString);

        public static SearchServiceClient CreateSearchServiceClient() => new SearchServiceClient(SearchUri, SearchCredentials, HttpClientHandler);
        public static CloudQueueClient CreateCloudQueueClient() => CloudStorageAccount.CreateCloudQueueClient();
        public static CloudTableClient CreateCloudTableClient() => CloudStorageAccount.CreateCloudTableClient();
        
        private static string GetSetting(string name) => CloudConfigurationManager.GetSetting(name) ?? Environment.GetEnvironmentVariable(name);
    }
}
