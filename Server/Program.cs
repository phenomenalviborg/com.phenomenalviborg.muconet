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

            // Testing the packet class
            MUCOPacket packet = new MUCOPacket(8);
            packet.Write(666);
            /* ... send over network ... */
            MUCOPacket receivedPacket = new MUCOPacket(packet.ToArray());
            int id = packet.ReadInt();
            int data = packet.ReadInt();
            int outOfRangeData = packet.ReadInt(); // Should give error, and return 0, but not crash.
            Console.WriteLine($"ID: {id} | Data: {data}, Out of range data: {outOfRangeData}");

            while (true) { }
        }

        private static void Log(MUCOLogMessage message)
        {
            Console.WriteLine(message);
        }
    }
}
