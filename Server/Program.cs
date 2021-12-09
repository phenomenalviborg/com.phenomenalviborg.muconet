﻿using System;
using System.Collections.Generic;
using PhenomenalViborg.MUCONet;

namespace Server
{
    enum ServerPackets : int
    {
        HelloFromServer
    }

    enum ClientPackets : int
    {
        HelloFromClient,
        UpdateTransform
    }

    class ServerProgram
    {
        public ServerProgram()
        {
            MUCOLogger.LogEvent += Log;

            MUCOServer server = new MUCOServer();
            server.RegisterPacketHandler((int)ClientPackets.HelloFromClient, HandleHelloFromClient);
            server.RegisterPacketHandler((int)ClientPackets.UpdateTransform, HandleUpdateTransform);
            server.Start(1000);

            server.OnClientConnectedEvent += OnClientConnected;
            server.OnClientDisconnectedEvent += OnClientDisconnected;

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

        private void HandleHelloFromClient(MUCOPacket packet, int fromClient)
        {
            Console.WriteLine("HelloFromClient");
        }

        private void HandleUpdateTransform(MUCOPacket packet, int fromClient)
        {
            Console.WriteLine("UpdateTransform");
        }

        #region Events
        private void OnClientConnected()
        {
            Console.WriteLine("OnClientConnected()");
        }

        private void OnClientDisconnected()
        {
            Console.WriteLine("OnClientDisconnected()");
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
