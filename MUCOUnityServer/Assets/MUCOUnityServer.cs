using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Phenomenal.MUCONet;

public class MUCOUnityServer : MonoBehaviour
{
    [SerializeField] private int m_ServerPort = 1000;

    public MUCOServer Server { get; private set; } = null;

    [SerializeField] private GameObject m_UserPrefab = null;
    private Dictionary<int, GameObject> m_UserObjects = new Dictionary<int, GameObject>();

    enum ServerPackets : int
    {
        HelloFromServer
    }

    enum ClientPackets : int
    {
        HelloFromClient,
        UpdateTransform
    }

    private void Start()
    {
        MUCOLogger.LogEvent += Log;

        Server = new MUCOServer();
        Server.RegisterPacketHandler((int)ClientPackets.HelloFromClient, HandleHelloFromClient);
        Server.RegisterPacketHandler((int)ClientPackets.UpdateTransform, HandleUpdateTransform);
        Server.Start(m_ServerPort);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    private void HandleHelloFromClient(MUCOPacket packet, int fromClient)
    {
        Debug.Log("HelloFromClient");
    }

    private void HandleUpdateTransform(MUCOPacket packet, int fromClient)
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            Debug.Log($"[UpdateTransform]: {fromClient}");

            if (m_UserObjects.ContainsKey(fromClient))
            {
                float newX = packet.ReadFloat();
                float newY = packet.ReadFloat();
                m_UserObjects[fromClient].transform.position = new Vector3(newX, newY, 0.0f);
            }
            else
            {
                m_UserObjects[fromClient] = Instantiate(m_UserPrefab);
            }
        });
    }

    private static void Log(MUCOLogMessage message)
    {
        Debug.Log(message.Message);
    }
}