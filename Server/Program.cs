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

            server.OnClientConnectedEvent += OnClientConnected;

            while (true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.D1)
                {
                    MUCOPacket packet = new MUCOPacket((int)ServerPackets.HelloFromServer);
                    server.SendPacketToAll(packet, true);
                }
                if (Console.ReadKey(true).Key == ConsoleKey.D2)
                {
                    Console.WriteLine("Client List:");
                    foreach (MUCOServer.MUCOClientInfo clientInfo in server.ClientInfo.Values)
                    {
                        Console.WriteLine($"{clientInfo.RemoteSocket.RemoteEndPoint} - ID: {clientInfo.UniqueIdentifier}");
                    }
                }
            }
        }

        private void HandleHelloFromClient(MUCOPacket packet)
        {
            Console.WriteLine("HelloFromClient");
        }

        #region Events
        private void OnClientConnected()
        {
            Console.WriteLine("OnClientConnected()");
        }
        #endregion

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
