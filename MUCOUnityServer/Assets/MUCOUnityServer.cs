using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Phenomenal.MUCONet;

public class MUCOUnityServer : MonoBehaviour
{
    enum ServerPackets : int
    {
        HelloFromServer
    }

    enum ClientPackets : int
    {
        HelloFromClient
    }

    private void Start()
    {
        MUCOLogger.LogEvent += Log;

        MUCOServer server = new MUCOServer();
        server.RegisterPacketHandler((int)ClientPackets.HelloFromClient, HandleHelloFromClient);
        server.Start(1000);
    }

    private void HandleHelloFromClient(MUCOPacket packet)
    {
        Debug.Log("HelloFromClient");
    }

    private static void Log(MUCOLogMessage message)
    {
        Debug.Log(message.Message);
    }
}