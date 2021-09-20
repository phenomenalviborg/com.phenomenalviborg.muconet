using System;
using Phenomenal.MUCONet;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            MUCOClient client = new MUCOClient();
            client.Connect();

            while (true) { }
        }
    }
}
