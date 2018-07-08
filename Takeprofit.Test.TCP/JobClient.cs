using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Takeprofit.Test.TCP
{
    public class JobClient
    {
        // Task array
        private Task[] _tasks;

        // Address of server
        private string _address;

        // TCP/IP port
        private int _port;

        // Max number sending to server
        private int _max;

        // Threads count
        private int _threads;

        // List of all numbers received
        private List<uint> _numbers;

        // Collection of numbers in queue waiting to be sended
        private static BlockingCollection<int> _queue = new BlockingCollection<int>();

        public JobClient(int threadsCount, int maxNumber = 2018)
        {
            // Parsing port from UNIX
            _port = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(1337012345).ToLocalTime().Year;
            _address = "job.latypoff.com";

            _threads = threadsCount;
            _max = maxNumber;
            _numbers = new List<uint>();
            _tasks = new Task[_threads];
        }

        private TcpClient GetClient()
        {
            // Connecting TCP
            var client = new TcpClient();
            client.Connect(_address, _port);

            return client;
        }

        public void Start()
        {
            // Filling up queue
            for (int i = 1; i <= _max; i++)
                _queue.Add(i);

            // Creating threads
            for (int i = 0; i < _threads; i++)
            {
                _tasks[i] = Task.Factory.StartNew(Work);
            }

            UpdateStatus();

            Task.WaitAll(_tasks);

            _numbers.Sort();

            if (_max % 2 == 1)
            {
                Console.WriteLine($"Median = {_numbers.ElementAt(_max / 2)}");
            }
            else
            {
                Console.WriteLine($"Median = {(_numbers.ElementAt(_max / 2 - 1) + _numbers.ElementAt(_max / 2)) / 2}");
            }
        }

        public void PrintTaskText()
        {
            // Sending message
            var data = Encoding.Default.GetBytes("Greetings\n");
            var stream = GetClient().GetStream();
            stream.Write(data, 0, data.Length);

            // Receiving response
            data = new Byte[2000];
            var bytes = stream.Read(data, 0, data.Length);
            Encoding encoding = Encoding.GetEncoding("KOI8-R");

            // Printing response text
            Console.WriteLine($"Text of the task:\n{encoding.GetString(data, 0, bytes)}");
        }

        private void UpdateStatus()
        {
            Console.Clear();
            Console.WriteLine($"{_numbers.Count}/{_max} completed.");
        }

        private void Work()
        {
            var stream = GetClient().GetStream();

            while (_queue.Count > 0)
            {
                // Sending message
                var data = Encoding.Default.GetBytes($"{_queue.Take()}\n");
                stream.Write(data, 0, data.Length);

                // Receiving response
                // P.S. Taking list to simplify reading bytes of unknown count
                var bytesList = new List<byte>();
                int symb;

                do
                {
                    symb = stream.ReadByte();
                    bytesList.Add((byte)symb);
                } while ((char)symb != '\n');
                // Casting to char just to be sure

                // Adding responsed number to sum
                lock(_numbers)
                    _numbers.Add(UInt32.Parse(Encoding.GetEncoding("UTF-8")
                        .GetString(bytesList.ToArray(), 0, bytesList.Count)
                        .Trim('\n', '\t', ' ', '.', '\r')));

                UpdateStatus();
            }
        }
    }
}
