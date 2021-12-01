using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Phenomenal.MUCONet;

public class MUCOUnityServer : MonoBehaviour
{
    public MUCOServer Server { get; private set; } = null;

    enum ServerPackets : int
    {
        HelloFromServer
    }

    enum ClientPackets : int
    {
        HelloFromClient
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    private void Awake()
    {
        MUCOLogger.LogEvent += Log;

        Server = new MUCOServer();
        Server.RegisterPacketHandler((int)ClientPackets.HelloFromClient, HandleHelloFromClient);
        Server.Start(1030);
   
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