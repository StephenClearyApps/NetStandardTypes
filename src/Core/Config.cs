using System;
using System.Net.Http;
using Microsoft.Azure;
using Microsoft.Azure.Search;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;

namespace NetStandardTypes
{
    public static class Config
    {
        private static string AzureSearchKey { get; } = GetSetting("NETSTANDARDTYPES_SEARCHKEY");
        private static string AzureStorageKey { get; } = GetSetting("NETSTANDARDTYPES_STORAGEKEY");

        private static Uri SearchUri { get; } = new Uri("https://netstandardtypes.search.windows.net");
        private static Uri QueueUri { get; } = new Uri("https://netstandardtypes.queue.core.windows.net/");
        private static HttpClientHandler HttpClientHandler { get; } = new HttpClientHandler();
        private static SearchCredentials SearchCredentials { get; } = new SearchCredentials(AzureSearchKey);
        private static StorageCredentials StorageCredentials { get; } = new StorageCredentials("netstandardtypes", AzureStorageKey);

        public static SearchServiceClient CreateSearchServiceClient() => new SearchServiceClient(SearchUri, SearchCredentials, HttpClientHandler);

        public static CloudQueueClient CreateCloudQueueClient() => new CloudQueueClient(QueueUri, StorageCredentials);

        private static string GetSetting(string name) => CloudConfigurationManager.GetSetting(name) ?? Environment.GetEnvironmentVariable(name);
    }
}
