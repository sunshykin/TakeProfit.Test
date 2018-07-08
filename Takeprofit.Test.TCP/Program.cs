using System;
using System.Collections.Concurrent;

namespace Takeprofit.Test.TCP
{
    class Program
    {
        private static BlockingCollection<int> Queue = new BlockingCollection<int>();

        static void Main(string[] args)
        {
            Console.WriteLine("Do you want to see task text? (Y/N)");
            var ans = Console.ReadLine();
            Console.WriteLine("How much threads should we use?");
            int threads = Int32.Parse(Console.ReadLine());
            Console.WriteLine("How much numbers should we send to the server? (Starting from 1)");
            int count = Int32.Parse(Console.ReadLine());


            // Creating client
            var client = new JobClient(threads, count);

            if (ans.ToUpper() == "Y")
            {
                client.PrintTaskText();
                Console.ReadKey();
            }
            client.Start();

            Console.WriteLine();
            Console.ReadKey();
        }
    }
}
