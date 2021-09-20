using System;
using Phenomenal.MUCONet;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            MUCOServer server = new MUCOServer();
            server.Start();

            while (true) { }
        }
    }
}
