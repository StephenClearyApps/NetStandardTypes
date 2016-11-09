#r ".\bin\NugetWalker.Logic.dll"
#r ".\bin\Core.dll"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using NetStandardTypes;
using NetStandardTypes.NugetWalker;

public static Task Run(TimerInfo myTimer, IAsyncCollector<IndexPackageRequest> processPackageQueue, TextWriter log)
{
    return EntryPoint.Run(myTimer, processPackageQueue, log);
}
