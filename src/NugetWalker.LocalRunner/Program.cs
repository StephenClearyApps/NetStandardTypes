using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NugetWalker.Logic;

namespace NugetWalker.LocalRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                EntryPoint.MainAsync().GetAwaiter().GetResult();
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }
    }
}
