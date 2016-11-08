using System;

namespace NetStandardTypes.NugetWalker.LocalRunner
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
