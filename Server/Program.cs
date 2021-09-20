using System;
using Phenomenal.MUCONet;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            MUCOLogger.LogEvent += Log;

            MUCOServer server = new MUCOServer();
            server.Start();

            while (true) { }
        }

        private static void Log(MUCOLogMessage message)
        {
            Console.WriteLine(message);
        }
    }
}
