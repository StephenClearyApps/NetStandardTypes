using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetStandardTypes;
using PackageIndexer.Logic;

namespace PackageIndexer.LocalRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                EntryPoint.Run(new IndexPackageRequest()
                {
                    PackageId = "Nito.Collections.Deque",
                    PackageVersion = "1.0.0",
                }, Console.Out);
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
