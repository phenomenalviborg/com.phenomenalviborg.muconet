using System;
using System.Collections.Generic;
using Phenomenal.MUCONet;

namespace Server
{
    enum ServerPackets : int
    {
        HelloFromServer
    }

    enum ClientPackets : int
    {
        HelloFromClient
    }


    class ServerProgram
    {
        public ServerProgram()
        {
            MUCOLogger.LogEvent += Log;

            MUCOServer server = new MUCOServer();
            server.RegisterPacketHandler((int)ClientPackets.HelloFromClient, HandleHelloFromClient);
            server.Start(1000);

            while (true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.D1)
                {
                    MUCOPacket packet = new MUCOPacket((int)ServerPackets.HelloFromServer);
                    server.SendPacketToAll(packet, true);
                }
            }
        }

        private void HandleHelloFromClient(MUCOPacket packet)
        {
            Console.WriteLine("HelloFromClient");
        }

        private static void Log(MUCOLogMessage message)
        {
            Console.WriteLine(message);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ServerProgram client = new ServerProgram();
        }
    }
}
