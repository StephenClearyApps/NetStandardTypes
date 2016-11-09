#r "NugetWalker.Logic"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using NetStandardTypes;

public static void Run(TimerInfo myTimer, IAsyncCollector<IndexPackageRequest> processPackageQueue, TextWriter log)
{
    log.WriteLine(myTimer);
}
