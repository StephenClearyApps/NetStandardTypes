using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace NetStandardTypes.NugetWalker.LocalRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                EntryPoint.Run(new RefreshCatalogRequest(), new QueueAsyncCollector<IndexPackageRequest>(Config.ProcessPackageQueueName), Console.Out).GetAwaiter().GetResult();
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        private sealed class QueueAsyncCollector<T> : IAsyncCollector<T>
        {
            private readonly CloudQueue _queue;

            public QueueAsyncCollector(string name)
            {
                _queue = Config.CreateCloudQueueClient().GetQueueReference(name);
            }

            public Task AddAsync(T item, CancellationToken cancellationToken = new CancellationToken())
            {
                return _queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(item)), cancellationToken);
            }
        }
    }
}
