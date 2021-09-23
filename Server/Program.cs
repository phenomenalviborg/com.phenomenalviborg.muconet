﻿using System;
using System.Collections.Generic;
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

            /*// Testing the packet class
            MUCOPacket packet = new MUCOPacket(8);
            packet.WriteInt(666);*/
            /* ... send over network ... */
            /*MUCOPacket receivedPacket = new MUCOPacket(packet.ToArray());
            int id = packet.ReadInt();
            int data = packet.ReadInt();
            int outOfRangeData = packet.ReadInt(); // Should give error, and return 0, but not crash.
            Console.WriteLine($"ID: {id} | Data: {data}, Out of range data: {outOfRangeData}");*/

            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.D1)
                {
                    MUCOPacket packet = new MUCOPacket(8);
                    packet.WriteInt(1234);

                    server.SendPacketToAll(packet, true);
                }
            }
        }

        private static void Log(MUCOLogMessage message)
        {
            Console.WriteLine(message);
        }
    }
}
