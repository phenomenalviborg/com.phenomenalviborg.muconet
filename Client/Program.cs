using System;
using Phenomenal.MUCONet;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            MUCOLogger.LogEvent += Log;

            MUCOClient client = new MUCOClient();
            client.Connect();

            while (true) { }
        }

        private static void Log(MUCOLogMessage message)
        {
            Console.WriteLine(message);
        }
    }
}
