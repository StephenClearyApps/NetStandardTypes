using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

public static void ProcessQueueMessage([QueueTrigger("queue")] string message, TextWriter log)
{
    log.WriteLine(message);
}
