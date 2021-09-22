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

            while (true) 
            {
                if (Console.ReadKey().Key == ConsoleKey.D1)
                {
                    MUCOPacket packet = new MUCOPacket(8);
                    packet.WriteInt(4321);

                    client.SendPacket(packet, true);
                }
            }
        }

        private static void Log(MUCOLogMessage message)
        {
            Console.WriteLine(message);
        }
    }
}
