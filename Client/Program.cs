using System;
using Phenomenal.MUCONet;

namespace Client
{
    enum ServerPackets : int
    {
        HelloFromServer
    }

    enum ClientPackets : int
    {
        HelloFromClient
    }

    class ClientProgram
    {
        public ClientProgram()
        {
            MUCOLogger.LogEvent += Log;

            MUCOClient client = new MUCOClient();
            client.RegisterPacketHandler((int)ServerPackets.HelloFromServer, HandleHelloFromServer);
            client.Connect();

            while (true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.D1)
                {
                    MUCOPacket packet = new MUCOPacket((int)ClientPackets.HelloFromClient);
                    client.SendPacket(packet, true);
                }
            }
        }

        private void HandleHelloFromServer(MUCOPacket packet)
        {
            Console.WriteLine("HelloFromServer");
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
            ClientProgram client = new ClientProgram();  
        }
    }
}
