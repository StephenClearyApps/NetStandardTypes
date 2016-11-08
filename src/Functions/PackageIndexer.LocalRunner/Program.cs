using System;

namespace NetStandardTypes.PackageIndexer.LocalRunner
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
