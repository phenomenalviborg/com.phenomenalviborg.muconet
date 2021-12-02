using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Phenomenal.MUCONet;

public class MUCOUnityServer : MonoBehaviour
{
    [SerializeField] private int m_ServerPort = 1000;

    public MUCOServer Server { get; private set; } = null;

    enum ServerPackets : int
    {
        HelloFromServer
    }

    enum ClientPackets : int
    {
        HelloFromClient,
        UpdateTransform
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    private void Start()
    {
        MUCOLogger.LogEvent += Log;

        Server = new MUCOServer();
        Server.RegisterPacketHandler((int)ClientPackets.HelloFromClient, HandleHelloFromClient);
        Server.RegisterPacketHandler((int)ClientPackets.UpdateTransform, HandleUpdateTransform);
        Server.Start(m_ServerPort);
    }

    private void HandleHelloFromClient(MUCOPacket packet)
    {
        Debug.Log("HelloFromClient");
    }

    private void HandleUpdateTransform(MUCOPacket packet)
    {
        float newX = packet.ReadFloat();
        Debug.Log($"[UpdateTransform] New X: {newX}");
    }

    private static void Log(MUCOLogMessage message)
    {
        Debug.Log(message.Message);
    }
}